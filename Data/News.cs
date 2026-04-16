using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public class News : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Excerpt { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string Category { get; set; } = "Góc tư vấn";

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; } = 0;
    }
}
