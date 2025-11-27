using sparkly_server.Domain.Projects;
using sparkly_server.Domain.Users;

namespace sparkly_server.Domain.Posts
{
    public class Post
    {
        public Guid Id { get; private set; }
        public Guid AuthorId { get; private set; }
        public Guid? ProjectId { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public User Author { get; private set; } = null!;
        public Project? Project { get; private set; }
        
        private Post() { }
        
        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public static Post CreateProjectUpdate(Guid authorId, Guid projectId, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Title and content are required.");
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                AuthorId = authorId,
                ProjectId = projectId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return post;
        }

        public static Post CreatePost(Guid projectId, Guid authorId, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Title and content are required.");
            }
            
            var post = new Post
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                AuthorId = authorId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return post;
        }

        public static Post UpdatePost(Post post, string title, string content)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("New title and content are required.");
            }

            post.Title = title.Trim();
            post.Content = content.Trim();
            post.Touch();

            return post;
        }

        public void EnsureCanBeDeletedBy(Guid authorId)
        {
            if (authorId == Guid.Empty) throw new ArgumentException("AuthorId is required.");
            if (authorId != AuthorId)
                throw new InvalidOperationException("Only the author can delete this post.");
        }
        
        public bool IsOwner(Guid userId) => AuthorId == userId;
    }
}
