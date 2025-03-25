using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;

namespace LampStoreProjects.Data
{
    public class Product
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int ReviewCount { get; set; } = 0;

        public string Tags { get; set; } = string.Empty;
        
        public int ViewCount { get; set; } = 0;

        public int Favorites { get; set; } = 0;
        public int SellCount { get; set; } = 0;
        
        public Guid? CategoryId { get; set; }

        public Category? Category { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool Status { get; set; } = false;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public ICollection<VariantType> VariantTypes { get; set; } = new List<VariantType>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        public CartItem? CartItem { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}