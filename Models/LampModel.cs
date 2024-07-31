using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class LampModel
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

        [Range(0, 100)]
        [Required]
        public int Quantity { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;

        public ICollection<LampImage> Images { get; set; } = new List<LampImage>();
    }

    public class LampImage
    {
        public int Id { get; set; }

        [Required]
        public string? ImagePath { get; set; }  // Lưu đường dẫn tới ảnh

        public int LampModelId { get; set; }

        public LampModel? LampModel { get; set; }
    }
}