using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using sparkly_server.Enum;
using sparkly_server.Infrastructure;
using sparkly_server.Services.Auth;
using sparkly_server.Services.Users;
using sparkly_server.Services.UserServices;
using System.Text;
using Scalar.AspNetCore;

namespace sparkly_server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
     
        
        var jwtKey = builder.Configuration["SPARKLY_JWT_KEY"]
                     ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_KEY")
                     ?? throw new Exception("JWT key missing");

        var jwtIssuer   = builder.Configuration["SPARKLY_JWT_ISSUER"]
                          ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_ISSUER")
                          ?? "sparkly";

        var jwtAudience = builder.Configuration["SPARKLY_JWT_AUDIENCE"]
                          ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_AUDIENCE")
                          ?? "sparkly-api";
        
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
        
        var connectionString = builder.Configuration.GetConnectionString("Default")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? throw new Exception("Connection string 'Default' not found.");

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        
        app.Run();
    }
}
