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

        // [HttpGet("")]
        // public async GetProjectGlobal([FromQuery] int take = 10, CancellationToken ct = default)
        // {
        //     
        // }
        
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _projects.CreateProjectAsync(request.ProjectName, request.Description, request.Visibility);

            var response = new ProjectResponse(
                Id: project.Id,
                ProjectName: project.ProjectName,
                Description: project.Description,
                Visibility: project.Visibility,
                OwnerId: project.OwnerId
            );
            
            return Ok(response);
        }

        // [HttpPut("update/{projectId:guid}")]
        // public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cn)
        // {
        //     var project = await _projects.GetProjectByIdAsync(projectId: id, cn);
        //
        //     project.up
        // }
    }
}
