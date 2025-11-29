using System.Security.Claims;

namespace sparkly_server.Services.Users.CurrentUser
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        /// Gets the unique identifier of the current user.
        /// This property retrieves the user's identifier, typically from the authentication context.
        /// If the user is not authenticated or the identifier cannot be parsed, this property returns null.
        /// The value is extracted from the claim associated with the user's identity using the `ClaimTypes.NameIdentifier`.
        /// It serves as a primary reference for identifying the user within the application.
        /// Returns:
        /// A `Guid?` that represents the user's unique identifier, or null if unavailable.
        public Guid? UserId =>
            Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        /// Gets the email address of the currently authenticated user.
        /// This property extracts the email information from the claims associated with the user's identity.
        /// If the user is not authenticated or no email claim is present, this property returns null.
        /// The value is retrieved using the `ClaimTypes.Email` claim type, as provided by the identity provider.
        /// Returns:
        /// A `string?` representing the user's email address, or null if unavailable.
        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

        /// Gets the username of the current user.
        /// This property retrieves the user's name, typically from the authentication context.
        /// If the user is not authenticated or the name claim is not available, this property returns null.
        /// The value is extracted from the claim associated with the user's identity using the `ClaimTypes.Name`.
        /// Returns:
        /// A `string?` that represents the user's username, or null if unavailable.
        public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);

        /// Gets the role of the current user.
        /// This property retrieves the role assigned to the user, typically from their identity claims.
        /// The value is derived from the claim associated with the `ClaimTypes.Role`.
        /// It can be used to determine the user's permissions or access level within the application.
        /// Returns:
        /// A `string?` representing the user's role, or null if the role is not specified.
        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

        /// Indicates whether the current user is authenticated.
        /// This property determines the authentication status of the user based on their associated identity.
        /// It checks the `IsAuthenticated` property of the user's identity within the claims principal.
        /// If the user is authenticated, this property returns true; otherwise, it returns false.
        /// This serves as a straightforward way to verify whether the user has successfully logged in or not.
        /// Returns:
        /// A boolean value indicating the user's authentication status: true if authenticated, false otherwise.
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        /// <summary>
        /// Determines whether the current user is in the specified role.
        /// </summary>
        /// <param name="role">The name of the role to check.</param>
        /// <returns>True if the user is in the specified role; otherwise, false.</returns>
        public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
    }
}
