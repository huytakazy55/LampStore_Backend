using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Models
{
    public class ProductImageModel
    {
        public Guid Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        [JsonIgnore]
        public ProductModel Product { get; set; } = new ProductModel();
    }
}