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

        [Range(0, double.MaxValue)]
        [Required]
        public double Price { get; set; }

        [Range(0, 1000)]
        [Required]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public CartItem? CartItem { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}