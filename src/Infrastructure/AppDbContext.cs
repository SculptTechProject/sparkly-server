using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain.Auth;
using sparkly_server.Domain.Posts;
using sparkly_server.Domain.Projects;
using sparkly_server.Domain.Users;
using sparkly_server.Enum;

namespace sparkly_server.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(cfg =>
            {
                cfg.ToTable("users");

                cfg.HasKey(u => u.Id);
                
                cfg.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                cfg.Property(u => u.UserName)
                    .IsRequired();
                
                cfg.HasIndex(u => u.Email)
                    .IsUnique();

                cfg.Property(u => u.PasswordHash)
                    .IsRequired();
                
                cfg.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Refresh Tokens
            modelBuilder.Entity<RefreshToken>(cfg =>
            {
                cfg.ToTable("refresh_tokens");

                cfg.HasKey(rt => rt.Id);

                cfg.Property(rt => rt.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                cfg.Property(rt => rt.CreatedAt)
                    .IsRequired();

                cfg.Property(rt => rt.ExpiresAt)
                    .IsRequired();
            });
            
            // Projects
            modelBuilder.Entity<Project>(cfg =>
                {
                    cfg.ToTable("projects");
                    
                    cfg.HasKey(p => p.Id);

                    cfg.HasIndex(p => p.ProjectName)
                        .IsUnique();
                    
                    cfg.Property(p => p.ProjectName)
                        .IsRequired()
                        .HasMaxLength(200);

                    cfg.Property(p => p.Description)
                        .HasMaxLength(2000);

                    cfg.Property(p => p.Slug)
                        .IsRequired()
                        .HasMaxLength(256);

                    cfg.Property(p => p.CreatedAt)
                        .IsRequired();

                    cfg.Property(p => p.UpdatedAt)
                        .IsRequired();

                    cfg.Property(p => p.OwnerId)
                        .IsRequired();

                    cfg.Property(p => p.Visibility)
                        .HasDefaultValue(ProjectVisibility.Private)
                        .IsRequired();
                    
                    cfg.HasMany(p => p.Members)
                        .WithMany(u => u.Projects)
                        .UsingEntity(j =>
                        {
                            j.ToTable("project_members");
                        });
                }
            );
            
            // Posts
            modelBuilder.Entity<Post>(cfg =>
            {
                cfg.ToTable("posts");

                cfg.HasKey(p => p.Id);

                cfg.Property(p => p.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                cfg.Property(p => p.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                cfg.Property(p => p.CreatedAt)
                    .IsRequired();

                cfg.Property(p => p.UpdatedAt)
                    .IsRequired();

                cfg.Property(p => p.AuthorId)
                    .IsRequired();

                // relation: Post -> Author (User)
                cfg.HasOne(p => p.Author)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                // relation: Post -> Project (optional)
                cfg.HasOne(p => p.Project)
                    .WithMany(pr => pr.Posts)
                    .HasForeignKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
