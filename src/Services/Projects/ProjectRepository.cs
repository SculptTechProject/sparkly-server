using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Projects;
using sparkly_server.Enum;
using sparkly_server.Infrastructure;

namespace sparkly_server.Services.Projects
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _db;
        
        public ProjectRepository(AppDbContext db)
        {
            _db = db;
        }
        
        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            await _db.Projects.AddAsync(project, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Projects
                .Include(p => p.Members) // jak masz relacje
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Project>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Projects
                .Where(p => p.OwnerId == userId || p.Members.Any(m => m.Id == userId))
                .ToListAsync(cancellationToken);
        }
        public async Task<bool> IsProjectNameTakenAsync(string projectName, CancellationToken cn)
        {
            return await _db.Projects
                .AnyAsync(pn => pn.ProjectName == projectName, cn);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _db.Projects
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Project>> GetRandomPublicAsync(int take, CancellationToken ct = default)
        {
            return await _db.Projects
                .Where(p => p.Visibility == ProjectVisibility.Public)
                .OrderBy(p => EF.Functions.Random()) 
                .Take(take)
                .ToListAsync(ct);
        }
    }
}
