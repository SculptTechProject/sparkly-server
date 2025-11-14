using sparkly_server.Domain;

namespace sparkly_server.Services.Users
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct = default);
        Task<User?> ValidateUserAsync(string identifier, string password, CancellationToken ct = default);
    }
}
