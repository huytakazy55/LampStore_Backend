using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Repositories.Chat;


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
        private readonly IOrderRepository _orderRepository;
        private readonly IChatRepository _chatRepository;

        public AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, IConfiguration configuration, IAccountRepository accountRepository, IOrderRepository orderRepository, IChatRepository chatRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _chatRepository = chatRepository;
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            try
            {
                if (string.IsNullOrEmpty(signInModel.Username) || string.IsNullOrEmpty(signInModel.Password))
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_EMPTY_CREDENTIALS));
                }

                var result = await _accountRepository.SignInAsync(signInModel);

                if (result == null)
                {
                    // Kiểm tra lại để xác định lỗi cụ thể
                    var user = await _userManager.FindByNameAsync(signInModel.Username);
                    if (user == null)
                    {
                        return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_INVALID_CREDENTIALS));
                    }
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_ACCOUNT_LOCKED));
                    }
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_INVALID_CREDENTIALS));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign in");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }
        [HttpPost("RequestSignUpOtp")]
        public async Task<IActionResult> RequestSignUpOtp(SignUpModel signUpModel)
        {
            try
            {
                var result = await _accountRepository.RequestSignUpOtpAsync(signUpModel);

                if (result.Succeeded)
                {
                    return Ok(new ApiSuccessResponse("Mã xác nhận OTP đã được gửi đến email của bạn."));
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.AUTH_REGISTER_FAILED, errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while requesting sign up OTP.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [HttpPost("SignUpVerifyOtp")]
        public async Task<IActionResult> SignUpVerifyOtp(SignUpVerifyOtpModel signUpVerifyOtpModel)
        {
            try
            {
                var result = await _accountRepository.SignUpVerifyOtpAsync(signUpVerifyOtpModel);

                if (result.Succeeded)
                {
                    return Ok(new ApiSuccessResponse("Đăng ký tài khoản thành công."));
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.AUTH_REGISTER_FAILED, errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying OTP and signing up.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
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
                    return Ok(new ApiSuccessResponse("Đăng ký tài khoản thành công."));
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.AUTH_REGISTER_FAILED, errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signing up.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [HttpPost("GoogleSignIn")]
        public async Task<IActionResult> GoogleSignIn(GoogleSignInModel googleSignInModel)
        {
            try
            {
                var result = await _accountRepository.GoogleSignInAsync(googleSignInModel);

                if (result == null)
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_GOOGLE_LOGIN_FAILED));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Google sign in.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [HttpPost("FacebookSignIn")]
        public async Task<IActionResult> FacebookSignIn(FacebookSignInModel facebookSignInModel)
        {
            try
            {
                var result = await _accountRepository.FacebookSignInAsync(facebookSignInModel);

                if (result == null)
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_FACEBOOK_LOGIN_FAILED));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Facebook sign in.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.RefreshToken))
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_REFRESH_TOKEN_REQUIRED));
                }

                var result = await _accountRepository.RefreshTokenAsync(model.RefreshToken);

                if (result == null)
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.AUTH_REFRESH_TOKEN_INVALID));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [Authorize]
        [HttpPost("RevokeRefreshToken")]
        public async Task<IActionResult> RevokeRefreshToken(RefreshTokenRequestModel model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
                }

                if (string.IsNullOrEmpty(model.RefreshToken))
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_REFRESH_TOKEN_REQUIRED));
                }

                var result = await _accountRepository.RevokeRefreshTokenAsync(model.RefreshToken, userId);

                if (!result)
                {
                    return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_REFRESH_TOKEN_NOT_FOUND));
                }

                return Ok(new ApiSuccessResponse("Thu hồi token làm mới thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [Authorize]
        [HttpGet("ActiveSessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
                }

                var sessions = await _accountRepository.GetActiveSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            }

            var profile = await _accountRepository.SyncUserProfileFromAccountAsync(userId);
            var user = await _accountRepository.GetUserAccountAsync(userId);

            if (profile == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_USER_NOT_FOUND));
            }

            var fallbackEmail = string.IsNullOrWhiteSpace(profile.Email)
                ? user?.Email ?? string.Empty
                : profile.Email;
            var fallbackPhoneNumber = string.IsNullOrWhiteSpace(profile.PhoneNumber)
                ? user?.PhoneNumber ?? string.Empty
                : profile.PhoneNumber;

            return Ok(new
            {
                profile.Id,
                profile.UserId,
                profile.FullName,
                Email = fallbackEmail,
                PhoneNumber = fallbackPhoneNumber,
                profile.Address,
                profile.ProfileAvatar,
                profile.CreatedAt,
                profile.UpdatedAt,
                profile.City,
                profile.CityName,
                profile.District,
                profile.DistrictName,
                profile.Ward,
                profile.WardName,
                AccountEmail = user?.Email ?? string.Empty,
                AccountUserName = user?.UserName ?? string.Empty,
                AccountPhoneNumber = user?.PhoneNumber ?? string.Empty
            });
        }

        [HttpGet("role/{userId}")]
        public async Task<IActionResult> GetRoleById(string userId)
        {
            var role = await _accountRepository.GetRolesByUserIdAsync(userId);
            if (role == null && !role!.Any())
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_ROLE_NOT_FOUND));
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
                return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            }

            var user = await _accountRepository.GetUserAccountAsync(userId);

            if (user == null)
            {
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_PROFILE_NOT_FOUND));
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
                return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_NO_USERS_FOUND));
            }

            return Ok(users);
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpPost("AssignRoles")]
        public async Task<IActionResult> AssignRoles(UpdateUserRolesModel model)
        {
            var result = await _accountRepository.UpdateUserRolesAsync(model);
            if (result.Succeeded)
            {
                return Ok(new ApiSuccessResponse("Cập nhật quyền thành công."));
            }

            return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, result.Errors.Select(e => e.Description)));
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("AvailableRoles")]
        public async Task<IActionResult> GetAvailableRoles()
        {
            var roles = await _accountRepository.GetAvailableRolesAsync();
            return Ok(roles);
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpPost("Roles")]
        public async Task<IActionResult> AddRole(RoleCreateModel model)
        {
            if (string.Equals(model.RoleName, AppRole.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_CANNOT_CREATE_SUPERADMIN));
            }
            var result = await _accountRepository.AddRoleAsync(model);
            if (result.Succeeded)
            {
                return Ok(new ApiSuccessResponse("Tạo quyền thành công."));
            }

            return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, result.Errors.Select(e => e.Description)));
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("AvailableMenus")]
        public async Task<IActionResult> GetAvailableMenus()
        {
            var menus = await _accountRepository.GetAvailableMenusAsync();
            return Ok(menus);
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("RoleMenus/{roleName}")]
        public async Task<IActionResult> GetRoleMenus(string roleName)
        {
            var menus = await _accountRepository.GetMenusByRoleAsync(roleName);
            return Ok(menus);
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpPost("RoleMenus/{roleName}")]
        public async Task<IActionResult> SetRoleMenus(string roleName, MenuPermissionsUpdateModel model)
        {
            var result = await _accountRepository.SetMenusForRoleAsync(roleName, model.Menus);
            if (result.Succeeded)
            {
                return Ok(new ApiSuccessResponse("Cập nhật menu thành công."));
            }

            return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, result.Errors.Select(e => e.Description)));
        }

        [Authorize]
        [HttpGet("UserMenus")]
        public async Task<IActionResult> GetUserMenus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
            }
            var menus = await _accountRepository.GetMenusForUserAsync(userId);
            return Ok(menus);
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

                return Ok(new ApiSuccessResponse("Tài khoản đã bị khóa."));
            }

            return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_USER_NOT_FOUND));
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

                return Ok(new ApiSuccessResponse("Tài khoản đã được mở khóa."));
            }

            return NotFound(ApiErrorResponse.FromCode(ErrorCodes.AUTH_USER_NOT_FOUND));
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
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));
                }

                await _accountRepository.LogoutAsync(userId);

                return Ok(new ApiSuccessResponse("Đăng xuất thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        /// <summary>
        /// Đổi mật khẩu (yêu cầu đăng nhập)
        /// </summary>
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));

                if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_CHANGE_PW_REQUIRED));

                if (model.NewPassword.Length < 6)
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_CHANGE_PW_MIN_LENGTH));

                var result = await _accountRepository.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new ApiSuccessResponse("Đổi mật khẩu thành công!"));
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                if (errors.Any(e => e.Contains("Incorrect password", StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_CHANGE_PW_WRONG));
                }

                return BadRequest(ApiErrorResponse.WithErrors(ErrorCodes.VALIDATION_FAILED, errors, string.Join(" ", errors)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
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
                    "success" => Ok(new ApiSuccessResponse("Mật khẩu mới đã được gửi đến email của bạn.")),
                    "user_not_found" => BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_USER_NOT_FOUND, "Không tìm thấy tài khoản với email hoặc tên đăng nhập này.")),
                    "no_email" => BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_PROFILE_NOT_FOUND, "Tài khoản này không có email được đăng ký.")),
                    "account_locked" => BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_ACCOUNT_LOCKED)),
                    "reset_failed" => StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR, "Không thể đặt lại mật khẩu. Vui lòng thử lại.")),
                    "email_failed" => StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR, "Đặt lại mật khẩu thành công nhưng không thể gửi email. Vui lòng liên hệ admin.")),
                    _ => StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing forgot password request.");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }

        /// <summary>
        /// Claim guest orders: assign all orders with the given GuestToken to the authenticated user.
        /// Called after login/signup from the frontend.
        /// </summary>
        [Authorize]
        [HttpPost("ClaimGuestOrders")]
        public async Task<IActionResult> ClaimGuestOrders([FromBody] ClaimGuestOrdersModel model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiErrorResponse.FromCode(ErrorCodes.UNAUTHORIZED));

                if (string.IsNullOrEmpty(model.GuestToken))
                    return BadRequest(ApiErrorResponse.FromCode(ErrorCodes.AUTH_GUEST_TOKEN_REQUIRED));

                var claimedOrders = await _orderRepository.ClaimGuestOrdersAsync(model.GuestToken, userId);

                // Also claim guest chats
                var claimedChats = 0;
                try
                {
                    claimedChats = await _chatRepository.ClaimGuestChatsAsync(model.GuestToken, userId);
                }
                catch (Exception chatEx)
                {
                    _logger.LogError(chatEx, "Error claiming guest chats");
                }

                return Ok(new ApiSuccessResponse(
                    $"Đã nhận {claimedOrders} đơn hàng và {claimedChats} cuộc hội thoại của khách.",
                    new { ClaimedOrders = claimedOrders, ClaimedChats = claimedChats }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming guest orders");
                return StatusCode(500, ApiErrorResponse.FromCode(ErrorCodes.INTERNAL_ERROR));
            }
        }
    }
}

public class ClaimGuestOrdersModel
{
    public string GuestToken { get; set; } = string.Empty;
}
