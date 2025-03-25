using AutoMapper;
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
        public DbSet<Product>? Products { get; set; }
        public DbSet<ProductImage>? ProductImages { get; set; }
        public DbSet<ProductVariant>? ProductVariants { get; set;}
        public DbSet<VariantType>? VariantTypes { get; set; }
        public DbSet<VariantValue>? VariantValues { get; set; }
        public DbSet<ProductVariantValue>? ProductVariantValues { get; set; }
        public DbSet<ProductReview>? ProductReviews { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderItem>? OrderItems { get; set; }
        public DbSet<Cart>? Carts { get; set; }
        public DbSet<CartItem>? CartItems { get; set; }
        public DbSet<Delivery>? Deliveries { get; set; }
        public DbSet<CheckIn>? CheckIns { get; set; }
        public DbSet<UserProfile>? UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductImage>()
                .HasOne(li => li.Product)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(li => li.Product)
                .WithMany(l => l.ProductVariants)
                .HasForeignKey(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ProductVariant>()
                .Property(o => o.DiscountPrice)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ProductVariant>()
                .Property(p => p.Price)
                .HasPrecision(18, 4);

            modelBuilder.Entity<VariantType>()
                .HasOne(li => li.Product)
                .WithMany(l => l.VariantTypes)
                .HasForeignKey(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VariantValue>()
                .HasOne(li => li.VariantType)
                .WithMany(l => l.Values)
                .HasForeignKey(li => li.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ProductReview>()
                .HasOne(li => li.Product)
                .WithMany(l => l.ProductReviews)
                .HasForeignKey(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .Property(o => o.Rating)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Product>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>()
                .HasMany(l => l.Products)
                .WithOne(li => li.Category)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(l => l.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(l => l.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.User)
                .WithMany(d => d.CheckIns)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Delivery>()
                .HasOne(l => l.Order)
                .WithMany(d => d.Deliveries)
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(i => i.OrderId)                
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.Price)
                .HasPrecision(18, 4);

            modelBuilder.Entity<OrderItem>()
                .HasOne(l => l.Product)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariantValue>()
                .HasOne(pvv => pvv.ProductVariant)
                .WithMany(pv => pv.ProductVariantValues)
                .HasForeignKey(pvv => pvv.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariantValue>()
                .HasOne(pvv => pvv.VariantValue)
                .WithMany(vv => vv.ProductVariantValues)
                .HasForeignKey(pvv => pvv.VariantValueId)
                .OnDelete(DeleteBehavior.NoAction);


        }
    }
}