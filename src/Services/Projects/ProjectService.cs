using sparkly_server.Domain.Projects;
using sparkly_server.DTO.Projects;
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

            if (await _projects.IsProjectNameTakenAsync(name, cancellationToken))
            {
                throw new InvalidOperationException("ProjectName already taken.");
            }
            
            var project = Project.Create(owner, name, description, visibility);
            
            await _projects.AddAsync(project, cancellationToken);
            
            return project;
        }
        
        public async Task<Project> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");

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
            if (project is null || string.IsNullOrWhiteSpace(newName))
                throw new InvalidOperationException("ProjectName not found");

            if (!project.IsOwner(userId))
                throw new UnauthorizedAccessException("You are not the owner of this project.");

            if (await _projects.IsProjectNameTakenAsync(newName, cancellationToken))
            {
                throw new InvalidOperationException("ProjectName already taken.");
            }
            
            project.Rename(newName);

            await _projects.SaveChangesAsync(cancellationToken);
        }
        
        public async Task ChangeDescriptionAsync(Guid projectId, string newDescription, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken);
            if (project is null)
                throw new InvalidOperationException("ProjectName not found");

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
                          ?? throw new InvalidOperationException("ProjectName not found");

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
                ?? throw new InvalidOperationException("ProjectName not found");
            
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
                          ?? throw new InvalidOperationException("ProjectName not found");
            
            var isAdmin = _currentUser.IsInRole(Roles.Admin);
            var isOwner = project.IsOwner(currentUser);
            
            if (!isAdmin && !isOwner)
                throw new UnauthorizedAccessException("You are not allowed to remove members from this project.");
            
            var userToRemove = await _users.GetByIdAsync(userId, cancellationToken)
                            ?? throw new InvalidOperationException("User not found");
            
            project.RemoveMember(userToRemove);
            
            await _projects.SaveChangesAsync(cancellationToken);
        }   
        
        public async Task<IReadOnlyList<ProjectResponse>> GetRandomPublicAsync(int take, CancellationToken ct = default)
        {
            var projects = await _projects.GetRandomPublicAsync(take, ct);

            return projects
                .Select(p => new ProjectResponse(p))
                .ToList();
        }
        
        public async Task UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");

            var isAdmin = _currentUser.IsInRole(Roles.Admin);
            var isOwner = project.IsOwner(userId);

            if (!isOwner && !isAdmin)
                throw new UnauthorizedAccessException("You are not allowed to edit this project.");

            if (!string.IsNullOrWhiteSpace(request.ProjectName) &&
                request.ProjectName != project.ProjectName)
            {
                if (await _projects.IsProjectNameTakenAsync(request.ProjectName, cancellationToken))
                    throw new InvalidOperationException("ProjectName already taken.");

                project.Rename(request.ProjectName);
            }

            if (request.Description is not null &&
                request.Description != project.Description)
            {
                project.ChangeDescription(request.Description);
            }

            if (request.Visibility != project.Visibility)
            {
                project.SetVisibility(request.Visibility);
            }

            await _projects.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");

            var isAdmin = _currentUser.IsInRole(Roles.Admin);
            var isOwner = project.IsOwner(userId);

            if (!isOwner && !isAdmin)
                throw new UnauthorizedAccessException("You are not allowed to delete this project.");

            await _projects.DeleteAsync(projectId, cancellationToken);
            
            await _projects.SaveChangesAsync(cancellationToken);
        }
    }
}
