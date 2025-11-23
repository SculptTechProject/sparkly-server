using Microsoft.Extensions.DependencyInjection;
using sparkly_server.DTO.Auth;
using sparkly_server.DTO.Projects;
using sparkly_server.Enum;
using sparkly_server.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace sparkly_server.test
{
    public class ProjectTest : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public ProjectTest(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
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
        /// Creates a new project asynchronously with the specified project name and returns the created project's details.
        /// </summary>
        /// <param name="projectName">The name of the project to be created.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the details of the created project, or null if deserialization fails.</returns>
        private async Task<ProjectResponse?> CreateProjectAsync(string projectName)
        {
            var payload = new CreateProjectRequest(
                ProjectName: projectName,
                Description: "Test project",
                Visibility: ProjectVisibility.Public
            );

            var response = await _client.PostAsJsonAsync("/api/v1/projects/create", payload);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<ProjectResponse>();
            return created;
        }

        /// <summary>
        /// Registers a new user and logs them in, setting the authentication token in the HTTP client header for subsequent requests.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the authentication response does not contain an access token.</exception>
        /// <returns>A task that represents the asynchronous operation of registering and logging in a user.</returns>
        private async Task RegisterAndLoginUser()
        {
            var email = "test@sparkly.local";
            var password = "Test1234!";
            var userName = "testuser";

            var registerPayload = new RegisterRequest(Username:userName, Email: email, Password: password);
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
            registerResponse.EnsureSuccessStatusCode();

            var loginPayload = new LoginRequest(Identifier: email, Password: password);
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
            
            loginResponse.EnsureSuccessStatusCode();

            var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            if (loginContent?.AccessToken is null)
            {
                throw new InvalidOperationException("Auth response did not contain access token");
            }

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginContent.AccessToken);
        }
        
        // Tests
        [Fact]
        public async Task CreateProject_Should_Create_Project_For_Authenticated_User()
        {
            await RegisterAndLoginUser();
            var projectName = "MyTestProject";

            var created = await CreateProjectAsync(projectName);

            Assert.NotNull(created);
            Assert.Equal(projectName, created.ProjectName);
        }
    }
}
