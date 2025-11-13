namespace sparkly_server.DTO
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string AccessToken, string RefreshToken);
}
