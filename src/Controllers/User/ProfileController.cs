using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.Services.Users.CurrentUser;

namespace sparkly_server.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/v1/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;

        public ProfileController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        /// <summary>
        /// Retrieves the current authenticated user's profile details such as UserId, Email, UserName, and Role.
        /// </summary>
        /// <returns>
        /// An HTTP 200 OK response containing the user's profile information if the user is authenticated.
        /// An HTTP 401 Unauthorized response if the user is not authenticated.
        /// </returns>
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized();

            return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.UserName, _currentUser.Role });
        }
    }
}
