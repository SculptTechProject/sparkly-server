using Sparkly.Tests.Infrastructure;
using System.Net;

namespace sparkly_server.Services.Users.test;

public class HealthzTest : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthzTest(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Healthz_ReturnsOk()
    {
        var response = await _client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
