using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class BannerModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        
        [StringLength(500)]
        public required string Description { get; set; }
        
        [Required]
        [StringLength(500)]
        public required string ImageUrl { get; set; }
        
        [StringLength(200)]
        public required string LinkUrl { get; set; }
        
        public int Order { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 