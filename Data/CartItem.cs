using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class CartItem : BaseEntity
    {
        public Guid? CartId { get; set; }
        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int Quantity { get; set; }
    }
}