using sparkly_server.Services.Users;

namespace sparkly_server.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtProvider _jwtProvider;

        public AuthService(IUserService userService, IJwtProvider jwtProvider)
        {
            _userService = userService;
            _jwtProvider = jwtProvider;
        }

        public async Task<AuthResult?> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _userService.ValidateUserAsync(email, password, ct);
            if (user is null)
            {
                return null;
            }

            var accessToken  = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            // TODO: save refresh token to db

            var now = DateTime.UtcNow;

            return new AuthResult(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessTokenExpiresAt: now.AddMinutes(15),
                RefreshTokenExpiresAt: now.AddDays(7)
            );
        }

        public Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        {
            // Revoke refresh token TODO
            throw new NotImplementedException();
        }
    }
}
