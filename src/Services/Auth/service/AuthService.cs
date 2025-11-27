using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Auth;
using sparkly_server.Infrastructure;
using sparkly_server.Services.Auth.provider;
using sparkly_server.Services.Users;

namespace sparkly_server.Services.Auth.service
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtProvider _jwtProvider;
        private readonly AppDbContext _db;

        public AuthService(IUserService userService, 
                           IJwtProvider jwtProvider,
                           AppDbContext db)
        {
            _userService = userService;
            _jwtProvider = jwtProvider;
            _db = db;
        }

        /// <summary>
        /// Authenticates a user based on the provided credentials and generates an authentication result containing tokens.
        /// </summary>
        /// <param name="identifier">The user's identifier, such as username or email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="ct">A cancellation token to observe while awaiting the task.</param>
        /// <returns>An AuthResult object containing the access token, refresh token, and their expiry times, or null if authentication fails.</returns>
        public async Task<AuthResult?> LoginAsync(string identifier, string password, CancellationToken ct = default)
        {
            var user = await _userService.ValidateUserAsync(identifier, password, ct);
            if (user is null)
            {
                return null;
            }

            var accessToken  = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            
            var now = DateTime.UtcNow;
            

            var refreshEntity = new RefreshToken(
                user.Id,
                refreshToken,
                now.AddDays(7)
            );

            _db.RefreshTokens.Add(refreshEntity);
            await _db.SaveChangesAsync(ct);

            return new AuthResult(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessTokenExpiresAt: now.AddMinutes(15),
                RefreshTokenExpiresAt: now.AddDays(7)
            );
        }

        /// <summary>
        /// Refreshes the user's authentication tokens by validating the provided refresh token and generating a new access token.
        /// </summary>
        /// <param name="refreshToken">The existing refresh token issued to the user for renewing authentication.</param>
        /// <param name="ct">A cancellation token to observe while performing the refresh operation.</param>
        /// <returns>An AuthResult object containing the new access token, the provided refresh token, and their respective expiry times. Returns null if the refresh token is invalid or inactive.</returns>
        public async Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var entity = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

            if (entity is null || !entity.IsActive)
            {
                return null;
            }

            var user = entity.User;

            var now = DateTime.UtcNow;

            var newAccessToken = _jwtProvider.GenerateAccessToken(user);

            return new AuthResult(
                AccessToken: newAccessToken,
                RefreshToken: entity.Token,
                AccessTokenExpiresAt: now.AddMinutes(15),
                RefreshTokenExpiresAt: entity.ExpiresAt
            );
        }

        /// <summary>
        /// Revokes a refresh token to log the user out by marking the token as revoked in the database.
        /// </summary>
        /// <param name="refreshToken">The token to be revoked, which identifies the user session.</param>
        /// <param name="ct">A cancellation token to observe while awaiting the task.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var entity = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

            if (entity is null)
            {
                return;
            }

            if (entity.RevokedAt is not null)
            {
                return;
            }

            entity.Revoke();
            await _db.SaveChangesAsync(ct);
        }
    }
}
