using sparkly_server.Domain.Posts;

namespace sparkly_server.Services.Posts.service
{
    public interface IPostService
    {
        Task<Post> AddPostAsync(Guid projectId, Guid userId, string title, string content);
        Task<Post?> GetPostByIdAsync(Guid id);
        Task DeletePostAsync(Guid postId, Guid userId);
        Task<IReadOnlyList<Post>> GetFeedPostAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetProjectPostsAsync(Guid projectId, CancellationToken ct = default);
    }
}
