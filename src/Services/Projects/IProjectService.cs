using sparkly_server.Domain.Projects;
using sparkly_server.DTO.Projects;
using sparkly_server.Enum;

namespace sparkly_server.Services.Projects
{
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(
            string name,
            string description,
            ProjectVisibility visibility,
            CancellationToken cancellationToken = default);
        
        Task<Project> GetProjectByIdAsync(
            Guid projectId, CancellationToken 
                cancellationToken = default);

        Task<IReadOnlyList<Project>> GetProjectsForCurrentUserAsync(
            CancellationToken cancellationToken = default);
        
        Task RenameAsync(
            Guid projectId,
            string newName,
            CancellationToken cancellationToken = default);

        Task ChangeDescriptionAsync(
            Guid projectId,
            string newDescription,
            CancellationToken cancellationToken = default);

        Task SetVisibilityAsync(
            Guid projectId,
            ProjectVisibility visibility,
            CancellationToken cancellationToken = default);

        Task AddMemberAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task RemoveMemberAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);
        
        Task<IReadOnlyList<ProjectResponse>> GetRandomPublicAsync(
            int take, 
            CancellationToken ct = default);

        Task UpdateProjectAsync(
            Guid projectId,
            UpdateProjectRequest request,
            CancellationToken cancellationToken = default);
        
        Task DeleteProjectAsync(
            Guid projectId, 
            CancellationToken cancellationToken = default);
    }
}
