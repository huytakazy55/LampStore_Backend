using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.Models;

namespace LampStoreProjects.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public new DbSet<User>? Users { get; set; }
        public DbSet<Lamp>? Lamps { get; set; }
        public DbSet<LampImage>? LampImages { get; set; }
        public DbSet<Category>? Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring relationships
            modelBuilder.Entity<LampImage>()
                .HasOne(li => li.Lamp)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.LampId);
        }
    }
}