using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.Services.Users;

namespace sparkly_server.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/v1/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;

        private ProfileController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized();

            return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.UserName, _currentUser.Role });
        }
    }
}
