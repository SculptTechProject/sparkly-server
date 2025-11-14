using System.Security.Claims;

namespace sparkly_server.Services.Users
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId =>
            Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
        public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);
        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
    }
}
