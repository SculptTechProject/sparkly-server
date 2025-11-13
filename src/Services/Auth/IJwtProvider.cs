using sparkly_server.Domain;

namespace sparkly_server.Services.Auth
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
