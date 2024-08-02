using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int Quantity { get; set; }
    }
}