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

        /// <summary>
        /// Adds a new project to the database asynchronously.
        /// </summary>
        /// <param name="project">The project entity to be added.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            await _db.Projects.AddAsync(project, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a project by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the project to be retrieved.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the project if found; otherwise, null.</returns>
        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Projects
                .Include(p => p.Members) // jak masz relacje
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Retrieves a list of projects associated with a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose projects are to be retrieved.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains a read-only list of projects associated with the specified user.</returns>
        public async Task<IReadOnlyList<Project>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Projects
                .Where(p => p.OwnerId == userId || p.Members.Any(m => m.Id == userId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Checks asynchronously whether a project name is already taken.
        /// </summary>
        /// <param name="projectName">The name of the project to check for existence.</param>
        /// <param name="cn">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains a boolean value indicating whether the project name is already taken.</returns>
        public async Task<bool> IsProjectNameTakenAsync(string projectName, CancellationToken cn)
        {
            return await _db.Projects
                .AnyAsync(pn => pn.ProjectName == projectName, cn);
        }

        /// <summary>
        /// Saves all changes made in the current context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes a project from the database asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the project to be deleted.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _db.Projects
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a random list of public projects asynchronously.
        /// </summary>
        /// <param name="take">The number of public projects to retrieve.</param>
        /// <param name="ct">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task that represents the asynchronous operation, containing a read-only list of public projects.</returns>
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
