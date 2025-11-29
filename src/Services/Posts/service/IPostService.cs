using sparkly_server.Domain.Posts;

namespace sparkly_server.Services.Posts.service
{
    public interface IPostService
    {
        Task<Post> AddProjectPostAsync(Guid authorId, Guid projectId, string title, string content, CancellationToken ct = default);
        Task<Post> AddFeedPostAsync(Guid authorId, string title, string content, CancellationToken ct = default);
        Task<Post?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Post?> UpdatePostAsync(Guid postId, Guid userId, string title, string content, CancellationToken ct = default);
        Task DeletePostAsync(Guid postId, Guid userId);
        Task<IReadOnlyList<Post>> GetFeedPostAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetProjectPostsAsync(Guid projectId, CancellationToken ct = default);
    }
}
