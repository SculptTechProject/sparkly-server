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

        public async Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct = default)
        {
            var userByEmail = await _users.GetByEmailAsync(email, ct);
            var userByName = await _users.GetByEmailAsync(userName, ct);
            
            if (userByEmail is not null || userByName is not null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Please provide valid name and email.");
            }
            
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                throw new InvalidOperationException("Password too weak.");
            }
            
            var user = new User(userName:userName, email:email, role:"user");
            
            var hash = _passwordHasher.HashPassword(user, password);
            user.SetPasswordHash(hash);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return user;
        }

        public async Task<User?> ValidateUserAsync(string identifier, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            User? user;

            if (identifier.Contains("@"))
            {
                // we treat it as email
                user = await _users.GetByEmailAsync(identifier, ct);
            }
            else
            {
                // we treat it as a username
                user = await _users.GetByUserNameAsync(identifier, ct);
            }

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
