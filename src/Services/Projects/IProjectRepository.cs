using sparkly_server.Domain.Projects;

namespace sparkly_server.Services.Projects
{
    public interface IProjectRepository
    {
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Project>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsProjectNameTakenAsync(string projectName, CancellationToken cn);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
