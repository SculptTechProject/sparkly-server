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

        /// <summary>
        /// Creates a new project with the specified details and assigns it to the authenticated user.
        /// </summary>
        /// <param name="name">The name of the project to be created.</param>
        /// <param name="description">A brief description of the project.</param>
        /// <param name="visibility">Specifies whether the project is public or private.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns the newly created project.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user is not authenticated, the owner is not found,
        /// or the project name is already taken.
        /// </exception>
        public async Task<Project> CreateProjectAsync(string name, string description, ProjectVisibility visibility,
                                                      CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Retrieves a project by its unique identifier.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns the project associated with the specified identifier.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the project with the specified identifier is not found.
        /// </exception>
        public async Task<Project> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var project = await _projects.GetByIdAsync(projectId, cancellationToken)
                          ?? throw new InvalidOperationException("Project not found");

            return project;
        }

        /// <summary>
        /// Retrieves the list of projects associated with the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns a read-only list of projects owned or shared with the current user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user is not authenticated.</exception>
        public Task<IReadOnlyList<Project>> GetProjectsForCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId
                         ?? throw new InvalidOperationException("User is not authenticated");
            return _projects.GetForUserAsync(userId, cancellationToken);
        }

        /// <summary>
        /// Renames an existing project using the specified new name.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to be renamed.</param>
        /// <param name="newName">The new name to assign to the project.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns a task that represents the asynchronous operation of renaming the project.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user is not authenticated, the project is not found, or the new name is invalid
        /// (e.g., null, empty, or already taken).
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the authenticated user is not the owner of the project.
        /// </exception>
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

        /// <summary>
        /// Changes the description of the specified project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project whose description will be changed.</param>
        /// <param name="newDescription">The new description to assign to the project.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>An asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user is not authenticated or if the project is not found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user is not the owner of the project.
        /// </exception>
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

        /// <summary>
        /// Sets the visibility of a specified project to either public or private.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project whose visibility is to be changed.</param>
        /// <param name="visibility">The new visibility setting for the project.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the current user is not authenticated or if the project cannot be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user does not own the project and is not an admin.
        /// </exception>
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

        /// <summary>
        /// Adds a new member to a project, ensuring the authenticated user has the necessary permissions.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to which the member will be added.</param>
        /// <param name="userId">The unique identifier of the user to add as a member of the project.</param>
        /// <param name="cancellationToken">A token to cancel the operation if necessary.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the authenticated user is not valid, the project is not found,
        /// or the user to be added does not exist.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the authenticated user does not have permission to add members to the project.
        /// </exception>
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

        /// <summary>
        /// Removes a member from a specified project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project from which the member will be removed.</param>
        /// <param name="userId">The unique identifier of the user to be removed from the project.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
        /// <returns>Returns a task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the current user is not authenticated, the project is not found, or the user to be removed is not found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the current user does not have sufficient permissions to remove members from the project.
        /// </exception>
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

        /// <summary>
        /// Retrieves a random selection of public projects.
        /// </summary>
        /// <param name="take">The number of public projects to retrieve.</param>
        /// <param name="ct">A token to cancel the operation if needed.</param>
        /// <returns>Returns a list of randomly selected public projects.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value of <paramref name="take"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the <paramref name="ct"/> token.
        /// </exception>
        public async Task<IReadOnlyList<ProjectResponse>> GetRandomPublicAsync(int take, CancellationToken ct = default)
        {
            var projects = await _projects.GetRandomPublicAsync(take, ct);

            return projects
                .Select(p => new ProjectResponse(p))
                .ToList();
        }

        /// <summary>
        /// Updates the details of an existing project with the provided information.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to update.</param>
        /// <param name="request">The updated project details including name, description, and visibility.</param>
        /// <param name="cancellationToken">A token to cancel the operation if required.</param>
        /// <returns>Returns a task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user is not authenticated or the project is not found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user does not have sufficient permissions to update the project.
        /// </exception>
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

        /// <summary>
        /// Deletes an existing project specified by its ID.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <param name="cancellationToken">A token to cancel the operation if necessary.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the authenticated user is not found or the project does not exist.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the user is neither the owner of the project nor an administrator.
        /// </exception>
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
