using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Lamp>? Lamps { get; set; }
        public DbSet<LampImage>? LampImages { get; set; }
        public DbSet<Category>? Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LampImage>()
                .HasOne(li => li.Lamp)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.LampId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lamp>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Lamps)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}