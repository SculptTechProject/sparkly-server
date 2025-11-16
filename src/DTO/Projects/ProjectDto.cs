namespace sparkly_server.DTO.Projects
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = default!;
        public List<MemberDto> Members { get; set; } = new();
    }
}
