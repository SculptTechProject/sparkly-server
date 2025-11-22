using Microsoft.Extensions.DependencyInjection;
using sparkly_server.DTO.Auth;
using sparkly_server.Infrastructure;
using Sparkly.Tests.Infrastructure;
using System.Text;
using System.Text.Json;

namespace sparkly_server.Services.Users.test
{
    public class UserTest : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public UserTest(TestWebApplicationFactory factory)
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
        
        private async Task RegisterTestUserAsync(string userName, string email, string password)
        {
            var payload = new RegisterRequest(Username: userName, Email: email, Password: password);

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/v1/auth/register", content);
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task User_CanBeCreated()
        {
            var userName = $"user_{Guid.NewGuid():N}";
            var email = $"{Guid.NewGuid():N}@example.com";
            var password = "UserPassword";

            await RegisterTestUserAsync(userName, email, password);
        }

        [Fact]
        public async Task User_CanLoginByEmail()
        {
            var userName = $"user_{Guid.NewGuid():N}";
            var email = $"{Guid.NewGuid():N}@example.com";
            var password = "UserPassword";

            await RegisterTestUserAsync(userName, email, password);

            var payload = new LoginRequest(Identifier: email, Password: password);

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/v1/auth/login", content);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task User_CanLoginByUsername()
        {
            var userName = $"user_{Guid.NewGuid():N}";
            var email = $"{Guid.NewGuid():N}@example.com";
            var password = "UserPassword";

            await RegisterTestUserAsync(userName, email, password);
            
            var payload = new LoginRequest(Identifier: userName, Password: password);

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/v1/auth/login", content);

            response.EnsureSuccessStatusCode();
        }
    }
}
