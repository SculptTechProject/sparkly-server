using sparkly_server.Domain.Posts;

namespace sparkly_server.Services.Posts.repo
{
    public interface IPostRepository
    {
        Task<IReadOnlyList<Post>> GetProjectPostsAsync(Guid projectId, CancellationToken ct = default);
        Task<Post> AddPostAsync(Post post, CancellationToken ct = default);
        Task<Post> UpdatePostAsync(Post post, CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetUserFeedPostsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetPostsForUserAsync(Guid userId, CancellationToken ct = default);
        Task<Post?> GetPostByIdAsync(Guid id, CancellationToken ct = default);
        Task DeletePostAsync(Guid id, CancellationToken ct = default);
    }
}
