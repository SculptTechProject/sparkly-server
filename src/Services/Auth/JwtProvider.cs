using Microsoft.IdentityModel.Tokens;
using sparkly_server.Domain.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace sparkly_server.Services.Auth
{
    public class JwtProvider : IJwtProvider
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtProvider(IConfiguration config)
        {
            _key      = config["SPARKLY_JWT_KEY"]     ?? throw new Exception("JWT key missing");
            _issuer   = config["SPARKLY_JWT_ISSUER"]  ?? "sparkly";
            _audience = config["SPARKLY_JWT_AUDIENCE"] ?? "sparkly-api";
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role),
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // na razie prosty generator; później można dodać zapisywanie do bazy
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
