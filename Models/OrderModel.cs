using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class OrderModel
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}