using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class DeliveryModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; }
    }
}