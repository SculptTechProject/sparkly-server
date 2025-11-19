using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.Domain.Projects;
using sparkly_server.DTO.Projects;
using sparkly_server.Enum;
using sparkly_server.Services.Projects;
using sparkly_server.Services.Users;

namespace sparkly_server.Controllers.Projects
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projects;
        private readonly ICurrentUser _currentUser;
        private readonly IUserService _users;
        
        public ProjectsController(IProjectService projects, ICurrentUser currentUser, IUserService users)
        {
            _projects = projects;
            _currentUser = currentUser;
            _users = users;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomProjects([FromQuery] int take = 20, CancellationToken ct = default)
        {
            var response = await _projects.GetRandomPublicAsync(take, ct);
            return Ok(response);
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _projects.CreateProjectAsync(request.ProjectName, request.Description, request.Visibility);

            var response = new ProjectResponse(project);
            
            return Ok(response);
        }

        [HttpGet("{projectId:guid}")]
        public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct = default)
        {
            var project = await _projects.GetProjectByIdAsync(projectId, ct);
            return Ok(project);
        }

        [HttpPut("update/{projectId:guid}")]
        public async Task<IActionResult> UpdateProject(
            Guid projectId,
            [FromBody] UpdateProjectRequest request,
            CancellationToken cn = default)
        {
            await _projects.UpdateProjectAsync(projectId, request, cn);
            return NoContent();
        }
    }
}
