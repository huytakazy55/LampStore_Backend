using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Models
{
    public class ProductModel
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

        public DateTime DateAdded { get; set; } = DateTime.Now;
        public bool Status { get; set; } = false;
        [JsonIgnore]
        public ICollection<ProductImageModel> Images { get; set; } = new List<ProductImageModel>();
        [JsonIgnore]
        public ICollection<ProductVariantModel> Variants { get; set; } = new List<ProductVariantModel>();
        [Precision(18, 2)]
        public decimal? MinPrice { get; set; }
        [Precision(18, 2)]
        public decimal? MaxPrice { get; set; }

        public int Stock {get;set;}
    }
}