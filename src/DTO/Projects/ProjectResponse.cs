using sparkly_server.Domain.Projects;
using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects
{
    public sealed record ProjectResponse
    {
        public Guid Id { get; init; }
        public string ProjectName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ProjectVisibility Visibility { get; init; }
        public Guid OwnerId { get; init; }
        
        public ProjectResponse()
        {
        }
        
        public ProjectResponse(Project project)
        {
            Id = project.Id;
            ProjectName = project.ProjectName;
            Description = project.Description;
            Visibility = project.Visibility;
            OwnerId = project.OwnerId;
        }
    }

}
