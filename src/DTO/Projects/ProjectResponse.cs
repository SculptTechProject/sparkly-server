using sparkly_server.Domain.Projects;
using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects
{
    public sealed record ProjectResponse
    {
        public Guid Id { get; }
        public string ProjectName { get; }
        public string Description { get; }
        public ProjectVisibility Visibility { get; }
        public Guid OwnerId { get; }

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
