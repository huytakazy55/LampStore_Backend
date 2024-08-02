using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class SignInModel
    {
        [MaxLength(100)]
        [Required]
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}