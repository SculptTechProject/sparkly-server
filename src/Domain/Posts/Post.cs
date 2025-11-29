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

        /// <summary>
        /// Updates the <see cref="UpdatedAt"/> property of the current post instance to the current UTC time.
        /// </summary>
        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new instance of the Post class using the specified parameters.
        /// </summary>
        /// <param name="authorId">The unique identifier of the author of the post.</param>
        /// <param name="projectId">The unique identifier of the associated project. Can be null for feed posts.</param>
        /// <param name="title">The title of the post. Must not be null or whitespace.</param>
        /// <param name="content">The content of the post. Must not be null or whitespace.</param>
        /// <returns>A newly created instance of the <see cref="Post"/> class.</returns>
        /// <exception cref="ArgumentException">Thrown if the title or content is null, empty, or whitespace.</exception>
        private static Post CreateInternal(Guid authorId, Guid? projectId, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Title and content are required.");
            }

            var now = DateTime.UtcNow;

            return new Post
            {
                Id = Guid.NewGuid(),
                AuthorId = authorId,
                ProjectId = projectId,
                Title = title.Trim(),
                Content = content.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>
        /// Creates a new project-specific update post using the specified parameters.
        /// </summary>
        /// <param name="authorId">The unique identifier of the author of the post.</param>
        /// <param name="projectId">The unique identifier of the associated project.</param>
        /// <param name="title">The title of the post. Must not be null or whitespace.</param>
        /// <param name="content">The content of the post. Must not be null or whitespace.</param>
        /// <returns>A newly created instance of the <see cref="Post"/> class representing a project update.</returns>
        /// <exception cref="ArgumentException">Thrown if the title or content is null, empty, or whitespace.</exception>
        public static Post CreateProjectUpdate(Guid authorId, Guid projectId, string title, string content)
            => CreateInternal(authorId, projectId, title, content);

        /// <summary>
        /// Creates a new feed post with the specified parameters.
        /// </summary>
        /// <param name="authorId">The unique identifier of the author of the feed post.</param>
        /// <param name="title">The title of the feed post. Must not be null or whitespace.</param>
        /// <param name="content">The content of the feed post. Must not be null or whitespace.</param>
        /// <returns>A newly created instance of the <see cref="Post"/> class representing the feed post.</returns>
        /// <exception cref="ArgumentException">Thrown if the title or content is null, empty, or whitespace.</exception>
        public static Post CreateFeedPost(Guid authorId, string title, string content)
            => CreateInternal(authorId, null, title, content);

        /// <summary>
        /// Updates the specified post with new title and content if the provided authorId matches the post's author.
        /// </summary>
        /// <param name="post">The post to be updated. Must not be null.</param>
        /// <param name="title">The new title for the post. Must not be null, empty, or whitespace.</param>
        /// <param name="content">The new content for the post. Must not be null, empty, or whitespace.</param>
        /// <param name="authorId">The unique identifier of the author attempting to update the post. Must match the post's AuthorId.</param>
        /// <returns>The updated instance of the <see cref="Post"/> class.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the authorId does not match the post's AuthorId.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the post is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the title or content is null, empty, or whitespace.</exception>
        public Post UpdatePost(Post post, string title, string content, Guid authorId)
        {
            if (authorId != post.AuthorId)
            {
                throw new InvalidOperationException("Only the author can update this post.");
            }

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

        /// <summary>
        /// Ensures that the specified user has permission to delete the post.
        /// </summary>
        /// <param name="authorId">The unique identifier of the user attempting to delete the post.</param>
        /// <exception cref="ArgumentException">Thrown if the provided authorId is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the provided authorId does not match the author's unique identifier for this post.</exception>
        public void EnsureCanBeDeletedBy(Guid authorId)
        {
            if (authorId == Guid.Empty) throw new ArgumentException("AuthorId is required.");
            if (authorId != AuthorId)
                throw new InvalidOperationException("Only the author can delete this post.");
        }

        /// <summary>
        /// Determines whether the specified user is the owner of the post.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to check ownership against.</param>
        /// <returns>True if the specified user is the owner of the post; otherwise, false.</returns>
        public bool IsOwner(Guid userId) => AuthorId == userId;
    }
}
