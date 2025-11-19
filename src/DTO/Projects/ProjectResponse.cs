using sparkly_server.Enum;

namespace sparkly_server.DTO.Projects
{
    public sealed record ProjectResponse(
        Guid Id,
        string ProjectName,
        string Description,
        ProjectVisibility Visibility,
        Guid OwnerId
    );
}
