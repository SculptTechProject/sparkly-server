namespace sparkly_server.DTO.Projects.Feed
{
    public sealed class ProjectFeedResult
    {
        public required IReadOnlyList<ProjectFeedItem> Items { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int TotalCount { get; init; }
    }
}
