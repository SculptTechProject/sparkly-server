using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects.Feed
{
    public class ProjectFeedItem
    {
        public Guid Id { get; init; }
        public required string ProjectName { get; init; }
        public string? Description { get; init; }
        public required string OwnerUserName { get; init; }
        public DateTime CreatedAt { get; init; }
        public ProjectVisibility Visibility { get; init; }
    }
}
