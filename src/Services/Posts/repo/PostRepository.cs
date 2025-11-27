using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Posts;
using sparkly_server.Infrastructure;

namespace sparkly_server.Services.Posts.repo
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _db;

        public PostRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Saves a new or updated post to the database asynchronously.
        /// </summary>
        /// <param name="post">The post entity to be saved.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The saved post entity.</returns>
        private async Task<Post> SavePostAsync(Post post, CancellationToken ct)
        {
            await _db.Posts.AddAsync(post, ct);
            await _db.SaveChangesAsync(ct);
            return post;
        }

        /// <summary>
        /// Retrieves all posts associated with a specific project, ordered by their creation date in descending order.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project for which posts are being retrieved.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A read-only list of posts associated with the specified project.</returns>
        public async Task<IReadOnlyList<Post>> GetProjectPostsAsync(Guid projectId, CancellationToken ct = default)
        {
            var posts = await _db.Posts
                .Where(p => p.ProjectId == projectId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);

            return posts;
        }

        /// <summary>
        /// Adds a new post to the database asynchronously.
        /// </summary>
        /// <param name="post">The post entity to be added.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The added post entity.</returns>
        public Task<Post> AddPostAsync(Post post, CancellationToken ct = default)
            => SavePostAsync(post, ct);

        /// <summary>
        /// Retrieves a collection of posts authored by a specific user, ordered by creation date in descending order, asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose posts are being retrieved.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A read-only list of posts authored by the specified user.</returns>
        public async Task<IReadOnlyList<Post>> GetPostsForUserAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.Posts
                .Where(p => p.AuthorId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves a paginated list of posts authored by the specified user, ordered by creation date in descending order.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose posts are to be retrieved.</param>
        /// <param name="page">The page number to retrieve, starting from 1.</param>
        /// <param name="pageSize">The number of posts to retrieve per page.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A read-only list of posts authored by the specified user for the given page.</returns>
        public async Task<IReadOnlyList<Post>> GetUserFeedPostsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            return await _db.Posts
                .Where(p => p.AuthorId == userId) // na start prosta wersja feedu
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves a post by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the post to retrieve.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The post entity if found; otherwise, null.</returns>
        public async Task<Post?> GetPostByIdAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
            return post;
        }

        /// <summary>
        /// Deletes a post from the database asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the post to be deleted.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeletePostAsync(Guid id, CancellationToken ct = default)
        {
            await _db.Posts
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(ct);
        }
    }
}
