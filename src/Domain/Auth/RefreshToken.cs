using sparkly_server.Domain.Users;

namespace sparkly_server.Domain.Auth
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Token { get; private set; } = default!;

        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public bool IsActive =>
            RevokedAt is null && DateTime.UtcNow <= ExpiresAt;

        public RefreshToken(Guid userId, string token, DateTime expiresAt)
        {
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Revokes the refresh token by marking it as no longer active.
        /// </summary>
        /// <param name="ip">The IP address from which the revoke operation is made.</param>
        /// <param name="replacedByToken">An optional new token that replaces the current token.</param>
        public void Revoke(string? ip = null, string? replacedByToken = null)
        {
            if (RevokedAt is not null)
            {
                return; // already revoked
            }

            RevokedAt = DateTime.UtcNow;
            RevokedByIp = ip;
            ReplacedByToken = replacedByToken;
        }
    }
}
