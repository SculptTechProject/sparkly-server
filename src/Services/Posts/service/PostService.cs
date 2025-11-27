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

        public Task<Post> AddPostAsync(Guid projectId, Guid userId, string title, string content)
        {
            var newPost = Post.CreatePost(projectId, userId, title, content);
            return _posts.AddPostAsync(newPost);
        }
        
        public Task<Post?> GetPostByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Post ID cannot be empty.", nameof(id));
            }

            return _posts.GetPostByIdAsync(id);
        }

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

        public Task<IReadOnlyList<Post>> GetFeedPostAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

            return _posts.GetUserFeedPostsAsync(userId, page, pageSize, ct);
        }

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
