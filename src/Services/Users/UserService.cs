using Microsoft.AspNetCore.Identity;
using sparkly_server.Domain.Users;

namespace sparkly_server.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(IUserRepository users)
        {
            _users = users;
        }

        /// <summary>
        /// Registers a new user with the specified username, email, and password.
        /// </summary>
        /// <param name="userName">The desired username for the new user.</param>
        /// <param name="email">The email address of the new user.</param>
        /// <param name="password">The password for the new user. Must be at least 6 characters long.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns an instance of <see cref="User"/> representing the newly created user.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the username or email is already registered,
        /// invalid input is provided, or the password does not meet the required criteria.
        /// </exception>
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

        /// <summary>
        /// Validates the credentials of a user using an identifier (email or username) and password.
        /// </summary>
        /// <param name="identifier">The unique identifier for a user, which can be either the username or email address.</param>
        /// <param name="password">The password to be validated for the specified user.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns an instance of <see cref="User"/> if the credentials are valid; otherwise, returns null.
        /// </returns>
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
