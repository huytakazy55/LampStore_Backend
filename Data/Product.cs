using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class Product
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public double? Weight { get; set; }

        [Range(0, 5)]
        public int? Rating { get; set; } = 5;

        public int? ReviewCount { get; set; } = 0;

        public string? Tags { get; set; }
        
        public int? ViewCount { get; set; } = 0;

        public int? Favorites { get; set; } = 0;
        public int? SellCount { get; set; } = 0;
        [MaxLength(1000)]
        [Required]
        public string? Materials { get; set; }
        [Range(0, double.MaxValue)]
        [Required]
        public double OriginalPrice { get; set; }
        [Range(0, 100)]
        public double? Discount { get; set; }
        [Range(0, double.MaxValue)]
        public double SalePrice { get; set; }
        [Range(0, 1000)]
        [Required]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public CartItem? CartItem { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}