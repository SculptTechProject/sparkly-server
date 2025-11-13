namespace sparkly_server.Services.Auth
{
    public record AuthResult(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt
    );

    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
        Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    }
}
