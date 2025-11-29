using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Users;
using sparkly_server.Infrastructure;

namespace sparkly_server.Services.Users.repo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _db.Users.AddAsync(user, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
