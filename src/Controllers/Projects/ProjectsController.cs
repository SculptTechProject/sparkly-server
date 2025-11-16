using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.Services.Projects;
using sparkly_server.Services.Users;

namespace sparkly_server.Controllers.Projects
{
    [ApiController]
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
        
        // Controllers soon :)
    }
}
