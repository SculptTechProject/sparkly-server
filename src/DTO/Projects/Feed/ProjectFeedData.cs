using sparkly_server.Domain.Projects;

namespace sparkly_server.DTO.Projects.Feed
{
    public sealed class ProjectFeedData
    {
        public required IReadOnlyList<Project> Items { get; init; }
        public required int TotalCount { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
    }
}
