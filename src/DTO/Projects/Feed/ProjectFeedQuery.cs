using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects.Feed
{
    public sealed class ProjectFeedQuery
    {
        public Guid? OwnerId { get; set; }
        public bool Mine { get; set; }
        public ProjectVisibility? Visibility { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
