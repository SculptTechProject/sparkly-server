namespace sparkly_server.Services.Users.CurrentUser
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? UserName { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
