using sparkly_server.Domain.Auth;

namespace sparkly_server.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = default!;
        public string UserName { get; set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public string Role { get; private set; } = "User";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private User() { }

        public User(string userName, string email, string role = "User")
        {
            Id = Guid.NewGuid();
            Email = email;
            UserName = userName;
            Role = role;
        }

        public void SetPasswordHash(string hash)
        {
            PasswordHash = hash;
        }
    }
}
