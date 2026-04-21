using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Helpers;

namespace LampStoreProjects.Data
{
    public class Banner : ITimestampEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(500)]
        public required string ImageUrl { get; set; }
        
        [StringLength(200)]
        public string? LinkUrl { get; set; }
        
        public int Order { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.VietnamNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 