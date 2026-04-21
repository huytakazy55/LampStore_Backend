using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Helpers;

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

        public DateTime CreatedAt { get; set; } = DateTimeHelper.VietnamNow;

        public DateTime? UpdatedAt { get; set; }
    }
} 