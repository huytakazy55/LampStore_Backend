using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyApiNetCore6.Data
{
    [Table("Lamp")]
    public class LampModel
    {
        [Key]
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
        
        [JsonIgnore]
        public ICollection<LampImage> Images { get; set; } = new List<LampImage>();
    }

    [Table("LampImage")]
    public class LampImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? ImagePath { get; set; }

        public int LampModelId { get; set; }

        [ForeignKey("LampModelId")]
        [JsonIgnore]
        public LampModel? LampModel { get; set; }
    }
}