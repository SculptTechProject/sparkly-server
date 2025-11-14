using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.DTO;
using sparkly_server.Services.Auth;
using sparkly_server.Services.Users;

namespace sparkly_server.Controllers
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            // Add validation email and password??
            await _userService.RegisterAsync(userName: request.Username, email: request.Email, password: request.Password, ct: ct);
            return NoContent();
        }

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
    }
}
