using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using sparkly_server.Enum;
using sparkly_server.Services.Auth;
using sparkly_server.Services.Users;
using sparkly_server.Services.UserServices;
using System.Text;

namespace sparkly_server;

public class Program
{
    public static void Main(string[] args)
    {
        
        var jwtKey      = Environment.GetEnvironmentVariable("SPARKLY_JWT_KEY")!;
        var jwtIssuer   = Environment.GetEnvironmentVariable("SPARKLY_JWT_ISSUER") ?? "sparkly";
        var jwtAudience = Environment.GetEnvironmentVariable("SPARKLY_JWT_AUDIENCE") ?? "sparkly-api";
        
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(Roles.Admin));
        });
        
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJwtProvider, JwtProvider>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var key = builder.Configuration["SPARKLY_JWT_KEY"]
                          ?? throw new Exception("JWT key missing");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
