using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    public interface ITimestampEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }

    public abstract class BaseEntity : ITimestampEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
} 