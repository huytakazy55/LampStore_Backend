using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class ProductImageModel
    {
        public Guid Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public Guid? ProductModelId { get; set; }

        public ProductModel ProductModel { get; set; } = new ProductModel();
    }
}