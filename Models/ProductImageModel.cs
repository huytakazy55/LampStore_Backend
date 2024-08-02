using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class ProductImageModel
    {
        public int Id { get; set; }

        [Required]
        public string? ImagePath { get; set; }

        public int ProductModelId { get; set; }

        public ProductModel? ProductModel { get; set; }
    }
}