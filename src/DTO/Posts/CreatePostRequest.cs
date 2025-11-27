namespace sparkly_server.DTO.Posts
{
    public record CreatePostRequest(Guid ProjectId, string Title, string Content);
}
