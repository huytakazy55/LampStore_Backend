using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LampStoreProjects.Models
{
    public class CheckInModel
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public string? Location { get; set; }
    }
}