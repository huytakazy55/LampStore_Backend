using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
    }
}