using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Auth;
using sparkly_server.Infrastructure;
using sparkly_server.Services.Users;

namespace sparkly_server.Services.Auth
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

        public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
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
