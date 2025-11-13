namespace sparkly_server.Domain.Auth
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Token { get; private set; } = default!;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool Revoked { get; private set; }

        public RefreshToken(string token, DateTime expiresAt)
        {
            Token = token;
            ExpiresAt = expiresAt;
        }

        public void Revoke() => Revoked = true;
    }
}
