using sparkly_server.Domain.Posts;

namespace sparkly_server.DTO.Posts.Mapper
{
    public static class PostMapper
    {
        /// <summary>
        /// Converts a Post domain object into a PostResponse DTO object.
        /// </summary>
        /// <param name="post">The Post domain object to convert.</param>
        /// <param name="currentUserId">The unique identifier of the current user to determine ownership.</param>
        /// <returns>A PostResponse object containing the mapped data and ownership-related properties.</returns>
        public static PostResponse ToResponse(this Post post, Guid currentUserId)
        {
            var isOwner = post.AuthorId == currentUserId;

            return new PostResponse
            {
                Id = post.Id,
                ProjectId = post.ProjectId ?? Guid.Empty,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                CanEdit = isOwner,
                CanDelete = isOwner
            };
        }
    }
}
