using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects
{
    public sealed record CreateProjectRequest(
        string ProjectName,
        string Description,
        ProjectVisibility Visibility
    );
}
