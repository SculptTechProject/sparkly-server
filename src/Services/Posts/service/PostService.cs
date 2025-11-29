using sparkly_server.Domain.Posts;
using sparkly_server.Services.Posts.repo;

namespace sparkly_server.Services.Posts.service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _posts;
        
        public PostService(IPostRepository posts)
        {
            _posts = posts;
        }

        /// <summary>
        /// Adds a new project-related post created by a specific author for a given project.
        /// </summary>
        /// <param name="authorId">The unique identifier of the author creating the post.</param>
        /// <param name="projectId">The unique identifier of the project associated with the post.</param>
        /// <param name="title">The title of the post.</param>
        /// <param name="content">The content of the post.</param>
        /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created post.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the provided parameters are invalid.</exception>
        public Task<Post> AddProjectPostAsync(Guid authorId, Guid projectId, string title, string content,
                                              CancellationToken ct = default)
        {
            var post = Post.CreateProjectUpdate(authorId, projectId, title, content);
            return _posts.AddPostAsync(post, ct);
        }

        /// <summary>
        /// Adds a new feed post created by a specific author.
        /// </summary>
        /// <param name="authorId">The unique identifier of the author creating the post.</param>
        /// <param name="title">The title of the feed post.</param>
        /// <param name="content">The content of the feed post.</param>
        /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created feed post.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided parameters are invalid.</exception>
        public Task<Post> AddFeedPostAsync(Guid authorId, string title, string content, CancellationToken ct = default)
        {
            var post = Post.CreateFeedPost(authorId, title, content);
            return _posts.AddPostAsync(post, ct);
        }

        /// <summary>
        /// Retrieves a post by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the post to retrieve.</param>
        /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the post if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided post ID is empty.</exception>
        public Task<Post?> GetPostByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Post ID cannot be empty.", nameof(id));
            }

            return _posts.GetPostByIdAsync(id, ct);
        }

        /// <summary>
        /// Updates an existing post with the specified details.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to be updated.</param>
        /// <param name="userId">The unique identifier of the user making the update.</param>
        /// <param name="title">The new title of the post.</param>
        /// <param name="content">The new content of the post.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated post, or null if the post is not found.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the post does not exist.</exception>
        public async Task<Post?> UpdatePostAsync(Guid postId, Guid userId, string title, string content, CancellationToken ct = default)
        {
            var post = await _posts.GetPostByIdAsync(postId, ct);

            if (post is null)
            {
                throw new InvalidOperationException("Post not found");
            }

            post.UpdatePost(post, title, content, userId);
            
            await _posts.UpdatePostAsync(post, ct);
            
            return post;
        }

        /// <summary>
        /// Deletes a post with the specified ID if the user is the owner.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to be deleted.</param>
        /// <param name="userId">The unique identifier of the user attempting to delete the post.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the post ID or user ID is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the post is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not the owner of the post.</exception>
        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            if (postId == Guid.Empty)
                throw new ArgumentException("Post ID cannot be empty.", nameof(postId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            var post = await _posts.GetPostByIdAsync(postId);

            if (post is null)
            {
                throw new InvalidOperationException("Post not found");
            }

            if (!post.IsOwner(userId))
            {
                throw new UnauthorizedAccessException("You are not the owner of this post.");
            }
            
            await _posts.DeletePostAsync(postId);
        }

        /// <summary>
        /// Retrieves a paginated list of feed posts for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom the feed posts are being retrieved.</param>
        /// <param name="page">The page number to retrieve. Must be greater than 0.</param>
        /// <param name="pageSize">The number of posts per page. Must be between 1 and 100.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of feed posts.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided user ID is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the page is less than or equal to 0, or if the page size is not between 1 and 100.</exception>
        public Task<IReadOnlyList<Post>> GetFeedPostAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");

            return pageSize is <= 0 or > 100 ? throw new ArgumentOutOfRangeException(nameof(pageSize),
                "Page size must be between 1 and 100.") : _posts.GetUserFeedPostsAsync(userId, page, pageSize, ct);

        }

        /// <summary>
        /// Retrieves all posts associated with the specified project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to retrieve posts for.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of posts associated with the given project.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided projectId is an empty GUID.</exception>
        public Task<IReadOnlyList<Post>> GetProjectPostsAsync(Guid projectId, CancellationToken ct = default)
        {
            if (projectId == Guid.Empty)
            {
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
            }
            
            return _posts.GetProjectPostsAsync(projectId, ct);
        }
    }
}
