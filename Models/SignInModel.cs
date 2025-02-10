using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class SignInModel
    {
        [MaxLength(100)]
        [Required]
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}