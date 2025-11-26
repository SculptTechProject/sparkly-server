using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace sparkly_server.test.config
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        
            builder.ConfigureAppConfiguration((config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["SPARKLY_JWT_KEY"] = "this-is-very-long-test-jwt-key-123456",
                    ["SPARKLY_JWT_ISSUER"] = "sparkly",
                    ["SPARKLY_JWT_AUDIENCE"] = "sparkly-api"
                };

                config.AddInMemoryCollection(settings);
            });
        }
    }
}
