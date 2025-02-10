using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class CartModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}