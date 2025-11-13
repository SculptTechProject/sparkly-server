using Microsoft.EntityFrameworkCore;
using sparkly_server.Domain;

namespace sparkly_server.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(cfg =>
            {
                cfg.ToTable("users");

                cfg.HasKey(u => u.Id);

                cfg.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                cfg.HasIndex(u => u.Email)
                    .IsUnique();

                cfg.Property(u => u.PasswordHash)
                    .IsRequired();
            });
        }
    }
}
