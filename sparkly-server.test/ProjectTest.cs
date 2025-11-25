using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sparkly_server.Domain.Users;
using sparkly_server.DTO.Projects;
using sparkly_server.Enum;
using sparkly_server.Infrastructure;
using sparkly_server.Services.Auth;
using sparkly_server.test.config;
using System.Net;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace sparkly_server.test
{
    public class ProjectTest : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;

        public ProjectTest(TestWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        public async Task InitializeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            db.Users.RemoveRange(db.Users);
            db.Projects.RemoveRange(db.Projects);
            await db.SaveChangesAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        // Helpers

        /// <summary>
        /// Authenticates a test user by creating a new user in the database,
        /// generating a JWT access token for the user, and assigning the token to the HTTP client.
        /// Returns the unique identifier of the created test user.
        /// </summary>
        /// <returns>A <see cref="Guid"/> representing the ID of the test user.</returns>
        private async Task<Guid> AuthenticateAsTestUserAsync()
        {
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();
            
            var user = new User(
                userName: "testuser",
                email: "test@sparkly.local"
            );
            
            user.SetPasswordHash("TEST_HASH");

            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            var token = jwtProvider.GenerateAccessToken(user);
            
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return user.Id;
        }

        /// <summary>
        /// Creates a new project with the specified name by sending a request to the server.
        /// The project is created with a default description and public visibility.
        /// Returns the details of the newly created project if the operation is successful.
        /// </summary>
        /// <param name="projectName">The name of the project to create.</param>
        /// <returns>A <see cref="ProjectResponse"/> containing the details of the created project, or null if creation fails.</returns>
        private async Task<ProjectResponse> CreateProjectAsync(string projectName)
        {
            var payload = new CreateProjectRequest(
                ProjectName: projectName,
                Description: "Test project",
                Visibility: ProjectVisibility.Public
            );

            var response = await _client.PostAsJsonAsync("/api/v1/projects/create", payload);

            var rawBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"[CreateProjectAsync] Status: {(int)response.StatusCode} {response.StatusCode}");
            _output.WriteLine($"[CreateProjectAsync] Body: {rawBody}");

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<ProjectResponse>();
            
            return created ?? throw new XunitException("CreateProjectAsync: response body deserialized to null");

        }

        // Tests

        [Fact]
        public async Task CreateProject_Should_Create_Project_For_Authenticated_User()
        {
            var userId = await AuthenticateAsTestUserAsync();
            var projectName = "MyTestProject";
            
            var created = await CreateProjectAsync(projectName);
            
            Assert.NotNull(created);
            Assert.Equal(projectName, created.ProjectName);
            
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var project = await db.Projects
                .AsNoTracking()
                .SingleAsync(p => p.Id == created.Id);

            Assert.Equal(projectName, project.ProjectName);
            Assert.Equal(userId, project.OwnerId);
        }

        [Fact]
        public async Task CreateProject_Should_Fail_For_Unauthenticated_User()
        {
            var request = new CreateProjectRequest("MyTestProject", "Test project", ProjectVisibility.Public);
            
            var response = await _client.PostAsJsonAsync("/api/v1/projects/create", request);
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task GetProjectById_Should_Return_Null_For_Nonexistent_Project()
        {
            var response = await _client.GetAsync("/api/v1/projects/1234567890abcdef");
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectById_Should_Return_Project()
        {
            await AuthenticateAsTestUserAsync();
            var projectName = "MyTestProject1";

            var project = await CreateProjectAsync(projectName);
            var projectId = project.Id;

            _output.WriteLine($"[Test] Created projectId = {projectId}");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var exists = await db.Projects.AnyAsync(p => p.Id == projectId);
                _output.WriteLine($"[Test] Project exists in DB: {exists}");
            }

            var response = await _client.GetAsync($"/api/v1/projects/{projectId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}