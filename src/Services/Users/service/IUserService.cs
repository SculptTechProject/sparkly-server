using sparkly_server.Domain.Users;

namespace sparkly_server.Services.Users.service
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct = default);
        Task<User?> ValidateUserAsync(string identifier, string password, CancellationToken ct = default);
    }
}
