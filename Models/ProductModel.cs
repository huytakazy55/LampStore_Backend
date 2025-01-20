using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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

        [Range(0, 5)]
        public int? Rating { get; set; } = 5;

        public int? ReviewCount { get; set; } = 0;

        public string? Tags { get; set; }
        
        public int? ViewCount { get; set; } = 0;

        public int? Favorites { get; set; } = 0;

        public int? SellCount { get; set; } = 0;

        public int? CategoryId { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;
        [JsonIgnore]
        public ICollection<ProductImageModel> Images { get; set; } = new List<ProductImageModel>();
        [JsonIgnore]
        public ICollection<ProductVariantModel> Variants { get; set; } = new List<ProductVariantModel>();
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? Quantity { get; set; }
    }
}