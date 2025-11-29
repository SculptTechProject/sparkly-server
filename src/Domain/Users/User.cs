using sparkly_server.Domain.Auth;
using sparkly_server.Domain.Posts;
using sparkly_server.Domain.Projects;
using System.ComponentModel.DataAnnotations;

namespace sparkly_server.Domain.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = default!;
        [MaxLength(20)]
        public string UserName { get; set; } = default!;
        [MaxLength(300)]
        public string PasswordHash { get; private set; } = default!;
        [MaxLength(20)]
        public string Role { get; private set; } = "User";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public ICollection<Project> Projects { get; private set; } = new List<Project>();
        public ICollection<Post> Posts { get; private set; } = new List<Post>();

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
