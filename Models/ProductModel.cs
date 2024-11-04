using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Models;

namespace LampStoreProjects.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public double? Weight { get; set; }

        [Range(0, 5)]
        public int? Rating { get; set; }

        public int? ReviewCount { get; set; }

        public string? Tags { get; set; }
        
        public int? ViewCount { get; set; }

        public int? Favorites { get; set; }
        public string? Materials { get; set; }
        [Range(0, double.MaxValue)]
        [Required]
        public double OriginalPrice { get; set; }
        [Range(0, double.MaxValue)]
        public double SalePrice { get; set; }
        public double? Discount { get; set; }

        [Range(0, 1000)]
        [Required]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;

        public ICollection<ProductImageModel> Images { get; set; } = new List<ProductImageModel>();
    }
}