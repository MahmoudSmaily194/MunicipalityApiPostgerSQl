using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Entities;
using System.Reflection.Emit;

namespace SawirahMunicipalityWeb.Data
{
    public class DBContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }
        public DbSet<Event> Events { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicesCategories> ServicesCategories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintIssue> ComplaintIssues { get; set; } 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>()
            .Property(u => u.Role)
                .HasConversion<string>();
            builder.Entity<News>()
          .HasIndex(n => n.Slug)
          .IsUnique();
            builder.Entity<Event>()
    .HasIndex(e => e.Slug)
    .IsUnique();

            builder.Entity<Service>()
                .HasIndex(s => s.Slug)
                .IsUnique();
            builder.Entity<RefreshToken>()
    .HasOne(rt => rt.User)
    .WithMany(u => u.RefreshTokens)
    .HasForeignKey(rt => rt.UserId);
        }
    }
}
