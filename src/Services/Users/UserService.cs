using Microsoft.AspNetCore.Identity;
using sparkly_server.Domain;
using sparkly_server.Services.Users;

namespace sparkly_server.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(IUserRepository users)
        {
            _users = users;
        }

        public async Task<User> RegisterAsync(string email, string password, CancellationToken ct = default)
        {
            var existing = await _users.GetByEmailAsync(email, ct);
            if (existing is not null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                throw new InvalidOperationException("Password too weak.");
            }
            
            var user = new User(email);
            
            var hash = _passwordHasher.HashPassword(user, password);
            user.SetPasswordHash(hash);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return user;
        }

        public async Task<User?> ValidateUserAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(email, ct);
            if (user is null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result is PasswordVerificationResult.Failed)
            {
                return null;
            }

            return user;
        }
    }
}
