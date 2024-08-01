using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LampStoreProjects.Data
{
    [Table("LampImage")]
    public class LampImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ImagePath { get; set; }

        public int LampId { get; set; }
        
        [ForeignKey("LampId")]
        [JsonIgnore]
        public Lamp Lamp { get; set; }
    }
}