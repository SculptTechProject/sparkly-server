namespace sparkly_server.DTO
{
    public record RegisterRequest(string Username, string Email, string Password);
    public record LoginRequest(string Identifier, string Password);
    public record AuthResponse(string AccessToken, string RefreshToken);
}
