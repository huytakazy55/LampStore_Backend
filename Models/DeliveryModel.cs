using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class DeliveryModel
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; }
    }
}