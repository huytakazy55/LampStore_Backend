using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using Microsoft.AspNetCore.Authentication;
using LampStoreProjects.Data;


namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;

        public AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, IConfiguration configuration, IAccountRepository accountRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _accountRepository = accountRepository;
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            try
            {
                var result = await _accountRepository.SignInAsync(signInModel);

                if (string.IsNullOrEmpty(result))
                {
                    return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
                }
                if (result == "blank")
                {
                    return Unauthorized("Tên đăng nhập hoặc mật khẩu không được để trống.");
                }
                if (result == "lockout")
                {
                    return Unauthorized("Tài khoản của bạn đã bị khóa! Liên hệ Admin để biết thêm thông tin");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            try
            {
                var result = await _accountRepository.SignUpAsync(signUpModel);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "User registered successfully." });
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signing up.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPost("GoogleSignIn")]
        public async Task<IActionResult> GoogleSignIn(GoogleSignInModel googleSignInModel)
        {
            try
            {
                var result = await _accountRepository.GoogleSignInAsync(googleSignInModel);

                if (string.IsNullOrEmpty(result))
                {
                    return Unauthorized("Đăng nhập Google thất bại.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Google sign in.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _accountRepository.GetUserProfileAsync(userId);

            if (profile == null)
            {
                return NotFound("Profile not found");
            }

            return Ok(profile);
        }

        [HttpGet("role/{userId}")]
        public async Task<IActionResult> GetRoleById(string userId)
        {
            var role = await _accountRepository.GetRolesByUserIdAsync(userId);
            if (role == null && !role!.Any())
            {
                return NotFound("Role not found");
            }
            return Ok(role);
        }

        [Authorize]
        [HttpGet("UserLogin")]
        public async Task<IActionResult> GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _accountRepository.GetUserAccountAsync(userId);

            if (user == null)
            {
                return NotFound("Profile not found");
            }

            return Ok(user);
        }

        [Authorize]
        [HttpGet("GetAllUserLogin")]
        public async Task<IActionResult> GetAllUserLogin()
        {
            var users = await _accountRepository.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NotFound("No users found");
            }

            return Ok(users);
        }

        [Authorize]
        [HttpPost("LockUser/{userId}")]
        public async Task<IActionResult> LockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Khóa tài khoản vô thời hạn
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                // Reset số lần đăng nhập thất bại (nếu cần)
                await _userManager.ResetAccessFailedCountAsync(user);

                return Ok("Tài khoản đã bị khóa.");
            }

            return NotFound("Người dùng không tồn tại.");
        }

        [Authorize]
        [HttpPost("UnLockUser/{userId}")]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Mở khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                // Reset số lần đăng nhập thất bại (nếu cần)
                await _userManager.ResetAccessFailedCountAsync(user);

                return Ok("Tài khoản đã được mở khóa.");
            }

            return NotFound("Người dùng không tồn tại.");
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _accountRepository.LogoutAsync(userId);

                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            try
            {
                var result = await _accountRepository.ForgotPasswordAsync(forgotPasswordModel);

                return result switch
                {
                    "success" => Ok(new { Message = "Mật khẩu mới đã được gửi đến email của bạn." }),
                    "user_not_found" => BadRequest(new { Message = "Không tìm thấy tài khoản với email hoặc tên đăng nhập này." }),
                    "no_email" => BadRequest(new { Message = "Tài khoản này không có email được đăng ký." }),
                    "account_locked" => BadRequest(new { Message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin." }),
                    "reset_failed" => StatusCode(500, new { Message = "Không thể đặt lại mật khẩu. Vui lòng thử lại." }),
                    "email_failed" => StatusCode(500, new { Message = "Đặt lại mật khẩu thành công nhưng không thể gửi email. Vui lòng liên hệ admin." }),
                    _ => StatusCode(500, new { Message = "Đã xảy ra lỗi. Vui lòng thử lại sau." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing forgot password request.");
                return StatusCode(500, new { Message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }
    }
}