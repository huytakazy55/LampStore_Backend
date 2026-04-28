using AutoMapper;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
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
        public DbSet<Tag>? Tags { get; set; }
        public DbSet<ProductTag>? ProductTags { get; set; }
        public DbSet<Banner>? Banners { get; set; }
        public DbSet<Chat>? Chats { get; set; }
        public DbSet<Message>? Messages { get; set; }
        public DbSet<WishlistItem>? WishlistItems { get; set; }
        public DbSet<News>? News { get; set; }
        public DbSet<SiteVisit>? SiteVisits { get; set; }
        public DbSet<FlashSale>? FlashSales { get; set; }
        public DbSet<FlashSaleItem>? FlashSaleItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thêm các index tùy chỉnh
            // Chuẩn hóa tên bảng
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");
            modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants");
            modelBuilder.Entity<VariantType>().ToTable("VariantTypes");
            modelBuilder.Entity<VariantValue>().ToTable("VariantValues");
            modelBuilder.Entity<ProductVariantValue>().ToTable("ProductVariantValues");
            modelBuilder.Entity<ProductReview>().ToTable("ProductReviews");
            modelBuilder.Entity<UserProfile>().ToTable("UserProfiles");
            modelBuilder.Entity<Tag>().ToTable("Tags");
            modelBuilder.Entity<ProductTag>().ToTable("ProductTags");
            modelBuilder.Entity<Banner>().ToTable("Banners");
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<WishlistItem>().ToTable("WishlistItems");
            modelBuilder.Entity<News>().ToTable("News");
            modelBuilder.Entity<SiteVisit>().ToTable("SiteVisits");
            modelBuilder.Entity<FlashSale>().ToTable("FlashSales");
            modelBuilder.Entity<FlashSaleItem>().ToTable("FlashSaleItems");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Status, p.CategoryId })
                .HasDatabaseName("IX_Products_Status_CategoryId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.UserId, o.Status })
                .HasDatabaseName("IX_Orders_UserId_Status");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .HasDatabaseName("IX_CartItems_CartId_ProductId");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("IX_Categories_Name");

            // Unique constraints
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .IsUnique(false);

            // Enforce 1-to-1: mỗi product chỉ có 1 variant
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.ProductId)
                .IsUnique()
                .HasFilter("[ProductId] IS NOT NULL")
                .HasDatabaseName("IX_ProductVariants_ProductId");

            // Cấu hình Add-on Product (self referencing)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.AddOnProduct)
                .WithMany()
                .HasForeignKey(p => p.AddOnProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email");

            // Thêm indexes cho tất cả các bảng còn lại
            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductId)
                .HasDatabaseName("IX_ProductImages_ProductId");

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .HasDatabaseName("IX_ProductVariants_SKU");

            modelBuilder.Entity<VariantType>()
                .HasIndex(vt => vt.ProductId)
                .HasDatabaseName("IX_VariantTypes_ProductId");

            modelBuilder.Entity<VariantValue>()
                .HasIndex(vv => vv.TypeId)
                .HasDatabaseName("IX_VariantValues_TypeId");

            modelBuilder.Entity<ProductReview>()
                .HasIndex(pr => pr.ProductId)
                .HasDatabaseName("IX_ProductReviews_ProductId");

            modelBuilder.Entity<ProductReview>()
                .HasIndex(pr => new { pr.ProductId, pr.Rating })
                .HasDatabaseName("IX_ProductReviews_ProductId_Rating");

            // Composite unique keys cho bảng nối/chi tiết
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId, ci.SelectedOptions })
                .IsUnique();
            modelBuilder.Entity<ProductTag>()
                .HasIndex(pt => new { pt.ProductId, pt.TagId })
                .IsUnique();
            modelBuilder.Entity<ProductVariantValue>()
                .HasIndex(pvv => new { pvv.ProductVariantId, pvv.VariantValueId })
                .IsUnique();

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Carts_UserId");

            modelBuilder.Entity<Delivery>()
                .HasIndex(d => d.OrderId)
                .HasDatabaseName("IX_Deliveries_OrderId");

            modelBuilder.Entity<CheckIn>()
                .HasIndex(ci => ci.UserId)
                .HasDatabaseName("IX_CheckIns_UserId");

            modelBuilder.Entity<CheckIn>()
                .HasIndex(ci => ci.CheckInDate)
                .HasDatabaseName("IX_CheckIns_CheckInDate");

            modelBuilder.Entity<UserProfile>()
                .HasIndex(up => up.UserId)
                .HasDatabaseName("IX_UserProfiles_UserId")
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .HasDatabaseName("IX_Tags_Name");

            modelBuilder.Entity<ProductTag>()
                .HasIndex(pt => pt.ProductId)
                .HasDatabaseName("IX_ProductTags_ProductId");

            modelBuilder.Entity<ProductTag>()
                .HasIndex(pt => pt.TagId)
                .HasDatabaseName("IX_ProductTags_TagId");

            modelBuilder.Entity<Banner>()
                .HasIndex(b => b.IsActive)
                .HasDatabaseName("IX_Banners_IsActive");

            modelBuilder.Entity<Chat>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Chats_UserId");

            modelBuilder.Entity<Chat>()
                .HasIndex(c => c.AssignedAdminId)
                .HasDatabaseName("IX_Chats_AssignedAdminId");

            modelBuilder.Entity<Chat>()
                .HasIndex(c => new { c.Status, c.Priority })
                .HasDatabaseName("IX_Chats_Status_Priority");

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ChatId)
                .HasDatabaseName("IX_Messages_ChatId");

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SenderId)
                .HasDatabaseName("IX_Messages_SenderId");

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ChatId, m.CreatedAt })
                .HasDatabaseName("IX_Messages_ChatId_CreatedAt");

            // WishlistItem indexes
            modelBuilder.Entity<WishlistItem>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_WishlistItems_UserId_ProductId");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(w => w.UserId)
                .HasDatabaseName("IX_WishlistItems_UserId");

            // WishlistItem relationships
            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductImage>()
                .HasOne(li => li.Product)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(li => li.Product)
                .WithOne(l => l.ProductVariant)
                .HasForeignKey<ProductVariant>(li => li.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ProductVariant>()
                .Property(o => o.DiscountPrice)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ProductVariant>()
                .Property(p => p.Price)
                .HasPrecision(18, 4);
            modelBuilder.Entity<OrderItem>()
                .Property(p => p.Price)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ProductReview>()
                .Property(p => p.Rating)
                .HasPrecision(5, 2);

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
                .HasPrecision(5, 2);

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

            modelBuilder.Entity<ProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Chat relationships
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.AssignedAdmin)
                .WithMany()
                .HasForeignKey(c => c.AssignedAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Message relationships
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // SiteVisit tracking relationship
            modelBuilder.Entity<SiteVisit>()
                .HasOne(sv => sv.Product)
                .WithMany() // Assuming no collection property needed on Product for visits
                .HasForeignKey(sv => sv.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SiteVisit>()
                .HasIndex(sv => sv.SessionId)
                .HasDatabaseName("IX_SiteVisits_SessionId");
            
            modelBuilder.Entity<SiteVisit>()
                .HasIndex(sv => sv.VisitedAt)
                .HasDatabaseName("IX_SiteVisits_VisitedAt");

            // FlashSale relationships
            modelBuilder.Entity<FlashSaleItem>()
                .HasOne(i => i.FlashSale)
                .WithMany(f => f.Items)
                .HasForeignKey(i => i.FlashSaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FlashSaleItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FlashSaleItem>()
                .HasIndex(i => new { i.FlashSaleId, i.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_FlashSaleItems_FlashSaleId_ProductId");

            modelBuilder.Entity<FlashSale>()
                .HasIndex(f => new { f.IsActive, f.StartTime, f.EndTime })
                .HasDatabaseName("IX_FlashSales_Active_Time");
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ITimestampEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (ITimestampEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTimeHelper.VietnamNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTimeHelper.VietnamNow;
                }
            }
        }
    }
}