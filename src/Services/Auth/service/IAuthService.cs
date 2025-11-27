namespace sparkly_server.Services.Auth.service
{
    public record AuthResult(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt
    );

    public interface IAuthService
    {
        Task<AuthResult?> LoginAsync(string identifier, string password, CancellationToken ct = default);
        Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    }
}
