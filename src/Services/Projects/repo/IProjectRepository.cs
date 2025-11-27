using sparkly_server.Domain.Projects;
using sparkly_server.DTO.Projects.Feed;

namespace sparkly_server.Services.Projects.repo
{
    public interface IProjectRepository
    {
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Project>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsProjectNameTakenAsync(string projectName, CancellationToken cn);
        Task<IReadOnlyList<Project>> GetRandomPublicAsync(int take, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProjectFeedData> GetQueryFeedAsync(ProjectFeedQuery query, Guid? currentUserId, CancellationToken cancellationToken = default);
    }
}
