using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects
{
    public sealed record UpdateProjectRequest(
        string ProjectName,
        string Description,
        ProjectVisibility Visibility
    );
}
