using sparkly_server.Domain;

namespace sparkly_server.Services.Users
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string email, string password, CancellationToken ct = default);
        Task<User?> ValidateUserAsync(string email, string password, CancellationToken ct = default);
    }
}
