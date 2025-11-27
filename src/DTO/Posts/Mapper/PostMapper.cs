using sparkly_server.Domain.Posts;

namespace sparkly_server.DTO.Posts.Mapper
{
    public static class PostMapper
    {
        /// <summary>
        /// Maps a <see cref="Post"/> domain object to a <see cref="PostResponse"/> DTO.
        /// </summary>
        /// <param name="post">The <see cref="Post"/> instance to be mapped.</param>
        /// <returns>A <see cref="PostResponse"/> representing the mapped data.</returns>
        public static PostResponse ToResponse(this Post post)
        {
            return new PostResponse
            {
                Id = post.Id,
                ProjectId = post.ProjectId ?? Guid.Empty,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }
    }
}
