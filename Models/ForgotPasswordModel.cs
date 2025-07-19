using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc tên đăng nhập")]
        public string EmailOrUsername { get; set; } = string.Empty;
    }
} 