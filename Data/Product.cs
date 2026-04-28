using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class Product : BaseEntity
    {

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int ReviewCount { get; set; } = 0;

        public string Tags { get; set; } = string.Empty;
        
        public int ViewCount { get; set; } = 0;

        public int Favorites { get; set; } = 0;
        public int SellCount { get; set; } = 0;
        
        public Guid? CategoryId { get; set; }

        public Category? Category { get; set; }

        public bool Status { get; set; } = true;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ProductVariant? ProductVariant { get; set; }
        public ICollection<VariantType> VariantTypes { get; set; } = new List<VariantType>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        public CartItem? CartItem { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

        public Guid? AddOnProductId { get; set; }
        [ForeignKey("AddOnProductId")]
        public Product? AddOnProduct { get; set; }
    }
}