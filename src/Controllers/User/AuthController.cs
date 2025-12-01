using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.DTO.Auth;
using sparkly_server.Services.Auth.service;
using sparkly_server.Services.Users.service;

namespace sparkly_server.Controllers.User
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public AuthController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="request">An object containing the user's username, email, and password.</param>
        /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
        /// <returns>An asynchronous operation result indicating the outcome of the registration process.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            // Add validation email and password??
            await _userService.RegisterAsync(userName: request.Username, email: request.Email, password: request.Password, ct: ct);
            return NoContent();
        }

        /// <summary>
        /// Authenticates a user with the provided login credentials.
        /// </summary>
        /// <param name="request">An object containing the user's identifier (username or email) and password.</param>
        /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
        /// <returns>An asynchronous operation result containing authentication tokens if successful, or an unauthorized response if credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request.Identifier, request.Password, ct);
            if (result is null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var response = new AuthResponse(
                AccessToken: result.AccessToken,
                RefreshToken: result.RefreshToken
            );

            return Ok(response);
        }

        /// <summary>
        /// Logs out a user by invalidating their refresh token.
        /// </summary>
        /// <param name="request">An object containing the refresh token to be invalidated.</param>
        /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
        /// <returns>An asynchronous operation result indicating the outcome of the logout process.</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
        {
            await _authService.LogoutAsync(request.RefreshToken, ct);
            return NoContent();
        }

        /// <summary>
        /// Issues a new access token and refresh token pair using a valid refresh token.
        /// </summary>
        /// <param name="request">An object containing the current refresh token.</param>
        /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
        /// <returns>An asynchronous result containing the newly issued tokens or an appropriate error response.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var result = await _authService.RefreshAsync(request.RefreshToken, ct);

            if (result is null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            return Ok(result);
        }

    }
}
