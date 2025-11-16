using sparkly_server.Domain.Projects;
using sparkly_server.Enum;
using sparkly_server.Services.Users;

namespace sparkly_server.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUser _currentUser;
        private readonly IProjectRepository _projects;

        public ProjectService(
            IUserRepository users,
            ICurrentUser currentUser,
            IProjectRepository projects)
        {
            _users = users;
            _currentUser = currentUser;
            _projects = projects;
        }

        public async Task<Project> CreateProjectAsync(string name, string description, ProjectVisibility visibility, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                ?? throw new InvalidOperationException("User is not authenticated");
            
            var owner = await _users.GetByIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Owner not found");
            
            var project = Project.Create(owner, name, description, visibility);
            
            await _projects.AddAsync(project, cancellationToken);
            
            return project;
        }
        
        public Task<Project> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var project = _projects.GetByIdAsync(projectId, cancellationToken);
            return project;
        }
        
        public Task<IReadOnlyList<Project>> GetProjectsForCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                ?? throw new InvalidOperationException("User is not authenticated");
            return _projects.GetForUserAsync(userId, cancellationToken);
        }
        
        public async Task RenameAsync(Guid projectId, string newName, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken);
            if (project is null)
                throw new InvalidOperationException("Project not found");

            if (!project.IsOwner(userId))
                throw new UnauthorizedAccessException("You are not the owner of this project.");

            project.Rename(newName);

            await _projects.SaveChangesAsync(cancellationToken);
        }
        
        public async Task ChangeDescriptionAsync(Guid projectId, string newDescription, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken);
            if (project is null)
                throw new InvalidOperationException("Project not found");

            if (!project.IsOwner(userId))
                throw new UnauthorizedAccessException("You are not the owner of this project.");

            project.ChangeDescription(newDescription);

            await _projects.SaveChangesAsync(cancellationToken);
        }
        
        public async Task SetVisibilityAsync(Guid projectId, ProjectVisibility visibility, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");

            var isAdmin = _currentUser.IsInRole(Roles.Admin);

            if (!project.IsOwner(userId) && !isAdmin)
                throw new UnauthorizedAccessException("You are not allowed to change visibility for this project.");

            project.SetVisibility(visibility);

            await _projects.SaveChangesAsync(cancellationToken);
        }
        
        public async Task AddMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
        {
            var currentUser = _currentUser.UserId
                ?? throw new InvalidOperationException("User is not authenticated");
            
            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                ?? throw new InvalidOperationException("Project not found");
            
            var isAdmin = _currentUser.IsInRole(Roles.Admin);
            var isOwner = project.IsOwner(currentUser);
            
            if (!isAdmin && !isOwner)
                throw new UnauthorizedAccessException("You are not allowed to add members to this project.");
            
            var userToAdd = await _users.GetByIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("User not found");
            
            project.AddMember(userToAdd);
            
            await _projects.SaveChangesAsync(cancellationToken);
        }
        
        public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
        {
            var currentUser = _currentUser.UserId
                              ?? throw new InvalidOperationException("User is not authenticated");
            
            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");
            
            var isAdmin = _currentUser.IsInRole(Roles.Admin);
            var isOwner = project.IsOwner(currentUser);
            
            if (!isAdmin && !isOwner)
                throw new UnauthorizedAccessException("You are not allowed to remove members from this project.");
            
            var userToRemove = await _users.GetByIdAsync(userId, cancellationToken)
                            ?? throw new InvalidOperationException("User not found");
            
            project.RemoveMember(userToRemove);
            
            await _projects.SaveChangesAsync(cancellationToken);
        }
    }
}
