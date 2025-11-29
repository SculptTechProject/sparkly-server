using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.DTO.Projects;
using sparkly_server.DTO.Projects.Feed;
using sparkly_server.Services.Projects.service;

namespace sparkly_server.Controllers.Projects
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projects;
        
        public ProjectsController(IProjectService projects) => _projects = projects;

        /// <summary>
        /// Retrieves a specified number of random public projects.
        /// </summary>
        /// <param name="take">The number of projects to retrieve. Defaults to 20 if not specified.</param>
        /// <param name="ct">A token to observe while waiting for the task to complete.</param>
        /// <returns>An HTTP response containing a list of random public projects.</returns>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomProjects([FromQuery] int take = 20, CancellationToken ct = default)
        {
            var response = await _projects.GetRandomPublicAsync(take, ct);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a feed of projects based on the specified query parameters.
        /// </summary>
        /// <param name="query">The query parameters used to filter and paginate the project feed.</param>
        /// <param name="ct">A token to observe while waiting for the task to complete.</param>
        /// <returns>An HTTP response containing the requested project feed.</returns>
        [HttpGet("feed")]
        public async Task<IActionResult> GetProjectsFeed(
            [FromQuery] ProjectFeedQuery query,
            CancellationToken ct = default)
        {
            var feed = await _projects.GetFeedAsync(query, ct);
            return Ok(feed);
        }


        /// <summary>
        /// Retrieves a project by its unique identifier.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to retrieve.</param>
        /// <param name="ct">A token to observe while waiting for the task to complete.</param>
        /// <returns>An HTTP response containing the project details.</returns>
        [HttpGet("{projectId:guid}")]
        public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct = default)
        {
            var project = await _projects.GetProjectByIdAsync(projectId, ct);

            if (project is null)
                return NotFound();

            return Ok(new ProjectResponse(project));
        }

        /// <summary>
        /// Creates a new project with the specified details.
        /// </summary>
        /// <param name="request">The request object containing the project name, description, and visibility settings.</param>
        /// <returns>A response containing the created project's details.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _projects.CreateProjectAsync(request.ProjectName, request.Description, request.Visibility);

            var response = new ProjectResponse(project);
            
            return Ok(response);
        }

        /// <summary>
        /// Updates the details of an existing project. (Admin project only)
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to be updated.</param>
        /// <param name="request">An object containing the updated project details, such as name, description, and visibility.</param>
        /// <param name="cn">A token to observe while waiting for the task to complete.</param>
        /// <returns>A no-content HTTP response indicating the project was successfully updated.</returns>
        [HttpPut("update/{projectId:guid}")]
        public async Task<IActionResult> UpdateProject(
            Guid projectId,
            [FromBody] UpdateProjectRequest request,
            CancellationToken cn = default)
        {
            await _projects.UpdateProjectAsync(projectId, request, cn);
            return NoContent();
        }

        /// <summary>
        /// Deletes a project identified by its unique ID. (Admin project only)
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <param name="ct">A token to observe while waiting for the task to complete.</param>
        /// <returns>An HTTP response with no content if the deletion was successful.</returns>
        [HttpDelete("delete/{projectId:guid}")]
        public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct = default)
        {
            await _projects.DeleteProjectAsync(projectId, ct);
            
            return NoContent();
        }
    }
}
