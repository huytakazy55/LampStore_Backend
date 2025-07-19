using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class Delivery : BaseEntity
    {
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; } = string.Empty;
    }
}