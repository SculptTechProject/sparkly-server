using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using sparkly_server.Enum;
using sparkly_server.Infrastructure;
using sparkly_server.Services.Auth;
using sparkly_server.Services.Auth.provider;
using sparkly_server.Services.Auth.service;
using sparkly_server.Services.Posts.repo;
using sparkly_server.Services.Posts.service;
using sparkly_server.Services.Projects;
using sparkly_server.Services.Projects.repo;
using sparkly_server.Services.Projects.service;
using sparkly_server.Services.Users;
using sparkly_server.Services.Users.CurrentUser;
using sparkly_server.Services.Users.repo;
using sparkly_server.Services.Users.service;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT configuration
var jwtKey = builder.Configuration["SPARKLY_JWT_KEY"]
             ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_KEY");

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        jwtKey = "this-is-very-long-test-jwt-key-123456";
    }
    else
    {
        throw new Exception("JWT key missing");
    }
}

var jwtIssuer = builder.Configuration["SPARKLY_JWT_ISSUER"]
                 ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_ISSUER")
                 ?? "sparkly";

var jwtAudience = builder.Configuration["SPARKLY_JWT_AUDIENCE"]
                   ?? Environment.GetEnvironmentVariable("SPARKLY_JWT_AUDIENCE")
                   ?? "sparkly-api";

// Common services
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Roles.Admin));
});

// Domain / app services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();

// Database
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseInMemoryDatabase("sparkly-tests");
    });
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                           ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                           ?? throw new Exception("Connection string 'Default' not found.");

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });
}

builder.Services.AddControllers();

// OpenAPI / Scalar
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("[JWT] Authentication failed:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("[JWT] Challenge fired:");
                Console.WriteLine($"Error: {context.Error}, Desc: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Run migrations only outside Testing
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("FrontendDev");

app.MapControllers();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;