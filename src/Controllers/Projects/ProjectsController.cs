using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.DTO.Projects;
using sparkly_server.Services.Projects;

namespace sparkly_server.Controllers.Projects
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projects;
        
        public ProjectsController(IProjectService projects) => _projects = projects;
        
        // Random projects to feed the homepage
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomProjects([FromQuery] int take = 20, CancellationToken ct = default)
        {
            var response = await _projects.GetRandomPublicAsync(take, ct);
            return Ok(response);
        }
        
        // Create project
        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _projects.CreateProjectAsync(request.ProjectName, request.Description, request.Visibility);

            var response = new ProjectResponse(project);
            
            return Ok(response);
        }

        // Get project by id
        [HttpGet("{projectId:guid}")]
        public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct = default)
        {
            var project = await _projects.GetProjectByIdAsync(projectId, ct);
            return Ok(project);
        }

        // Update project by id (admin only)
        [HttpPut("update/{projectId:guid}")]
        public async Task<IActionResult> UpdateProject(
            Guid projectId,
            [FromBody] UpdateProjectRequest request,
            CancellationToken cn = default)
        {
            await _projects.UpdateProjectAsync(projectId, request, cn);
            return NoContent();
        }
        
        // Delete project by id (admin only)
        [HttpDelete("delete/{projectId:guid}")]
        public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct = default)
        {
            await _projects.DeleteProjectAsync(projectId, ct);
            
            return NoContent();
        }
    }
}
