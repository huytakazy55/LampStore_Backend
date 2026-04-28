using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Data
{
    public class ProductAddOn
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public Guid AddOnProductId { get; set; }
        [ForeignKey("AddOnProductId")]
        public Product AddOnProduct { get; set; } = null!;

        public int SortOrder { get; set; } = 0;
    }
}
