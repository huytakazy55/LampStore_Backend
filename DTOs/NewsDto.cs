using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.DTOs
{
    public class NewsCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Excerpt { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string Category { get; set; } = "Góc tư vấn";

        public bool IsActive { get; set; } = true;
    }

    public class NewsUpdateDto : NewsCreateDto
    {
    }

    public class NewsDto : NewsCreateDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; }
    }
}
