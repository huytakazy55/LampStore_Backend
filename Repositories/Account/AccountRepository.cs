using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using LampStoreProjects.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly IConfiguration configuration;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly ApplicationDbContext context;
		private readonly IEmailService emailService;
		private readonly IMemoryCache memoryCache;

		public AccountRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IEmailService emailService, IMemoryCache memoryCache)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.roleManager = roleManager;
			this.context = context;
			this.emailService = emailService;
			this.memoryCache = memoryCache;
		}

		public async Task<TokenResponseModel?> SignInAsync(SignInModel model)
		{
			if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
			{
				return null; // Sẽ xử lý "blank" ở controller
			}

			var user = await userManager.FindByNameAsync(model.Username);
			if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
			{
				return null;
			}

			if (await userManager.IsLockedOutAsync(user))
			{
				return null; // Sẽ xử lý "lockout" ở controller
			}

			// Tạo Access Token và Refresh Token
			var accessToken = await CreateAccessTokenAsync(user);
			var refreshToken = await CreateAndSaveRefreshTokenAsync(user.Id);

			return new TokenResponseModel
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresIn = 10800, // 3 giờ (seconds)
				TokenType = "Bearer"
			};
		}

		public async Task<TokenResponseModel?> GoogleSignInAsync(GoogleSignInModel model)
		{
			using var transaction = await context.Database.BeginTransactionAsync();
			try
			{
				if (string.IsNullOrEmpty(model.Email))
				{
					return null;
				}

				// Tìm user bằng email
				var user = await userManager.FindByEmailAsync(model.Email);

				if (user == null)
				{
					// Tạo user mới từ Google account
					user = new ApplicationUser
					{
						UserName = model.Email, // Sử dụng email làm username
						Email = model.Email,
						GoogleUserId = model.GoogleUserId, // Lưu Google User ID
						LockoutEnabled = false, // Google account thường không cần lock
						EmailConfirmed = true   // Google account đã xác thực email
					};

					// Tạo user với random password (vì dùng Google OAuth)
					var result = await userManager.CreateAsync(user, Guid.NewGuid().ToString() + "Ict@#$123");
					if (!result.Succeeded)
					{
						await transaction.RollbackAsync();
						return null;
					}

					// Tạo UserProfile
					var userProfile = new UserProfile
					{
						UserId = user.Id,
						User = user,
						FullName = model.Name,
						ProfileAvatar = model.Picture
					};

					await context.UserProfiles!.AddAsync(userProfile);

					// Thêm role Customer
					if (!await roleManager.RoleExistsAsync(AppRole.Customer))
					{
						await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
					}
					await userManager.AddToRoleAsync(user, AppRole.Customer);
					
					// Generate welcome discount code
					await GenerateWelcomeDiscountCodeAsync(user.Id);
				}
				else
				{
					// User đã tồn tại, cập nhật GoogleUserId nếu chưa có
					if (string.IsNullOrEmpty(user.GoogleUserId))
					{
						user.GoogleUserId = model.GoogleUserId;
						await userManager.UpdateAsync(user);
					}
				}

				// Tạo JWT token
				var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id),
					new Claim(ClaimTypes.Name, user.UserName ?? ""),
					new Claim(ClaimTypes.Email, user.Email ?? ""),
					new Claim("GoogleUserId", model.GoogleUserId),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};

				var userRoles = await userManager.GetRolesAsync(user);
				foreach (var role in userRoles)
				{
					authClaims.Add(new Claim(ClaimTypes.Role, role));
				}

				var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.")));
				var tokenHandler = new JwtSecurityTokenHandler();
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(authClaims),
					Expires = DateTime.UtcNow.AddHours(3),
					NotBefore = DateTime.UtcNow,
					SigningCredentials = new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature),
					Issuer = configuration["Jwt:Issuer"],
					Audience = configuration["Jwt:Audience"]
				};

				var accessToken = await CreateAccessTokenAsync(user);
				var refreshToken = await CreateAndSaveRefreshTokenAsync(user.Id);

				await transaction.CommitAsync();

				return new TokenResponseModel
				{
					AccessToken = accessToken,
					RefreshToken = refreshToken,
					ExpiresIn = 50, // 1 phút (seconds)
					TokenType = "Bearer"
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				Console.WriteLine($"Google SignIn Error: {ex.Message}");
				return null;
			}
		}

		public async Task<TokenResponseModel?> FacebookSignInAsync(FacebookSignInModel model)
		{
			using var transaction = await context.Database.BeginTransactionAsync();
			try
			{
				if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.FacebookUserId))
				{
					return null;
				}

				var user = await userManager.Users.FirstOrDefaultAsync(u => u.FacebookUserId == model.FacebookUserId);
				user ??= await userManager.FindByEmailAsync(model.Email);

				if (user == null)
				{
					user = new ApplicationUser
					{
						UserName = model.Email,
						Email = model.Email,
						FacebookUserId = model.FacebookUserId,
						LockoutEnabled = false,
						EmailConfirmed = true
					};

					var result = await userManager.CreateAsync(user, Guid.NewGuid().ToString() + "Ict@#$123");
					if (!result.Succeeded)
					{
						await transaction.RollbackAsync();
						return null;
					}

					var userProfile = new UserProfile
					{
						UserId = user.Id,
						User = user,
						FullName = model.Name,
						ProfileAvatar = model.Picture
					};

					await context.UserProfiles!.AddAsync(userProfile);

					if (!await roleManager.RoleExistsAsync(AppRole.Customer))
					{
						await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
					}
					await userManager.AddToRoleAsync(user, AppRole.Customer);
					
					// Generate welcome discount code
					await GenerateWelcomeDiscountCodeAsync(user.Id);
				}
				else
				{
					if (string.IsNullOrEmpty(user.FacebookUserId))
					{
						user.FacebookUserId = model.FacebookUserId;
						await userManager.UpdateAsync(user);
					}
				}

				await context.SaveChangesAsync();

				var accessToken = await CreateAccessTokenAsync(user);
				var refreshToken = await CreateAndSaveRefreshTokenAsync(user.Id);

				await transaction.CommitAsync();

				return new TokenResponseModel
				{
					AccessToken = accessToken,
					RefreshToken = refreshToken,
					ExpiresIn = 10800,
					TokenType = "Bearer"
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				Console.WriteLine($"Facebook SignIn Error: {ex.Message}");
				return null;
			}
		}

		public async Task<IdentityResult> RequestSignUpOtpAsync(SignUpModel model)
		{
			if (string.IsNullOrEmpty(model.Email))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Email là bắt buộc." });
			}

			if (string.IsNullOrEmpty(model.Username))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên đăng nhập là bắt buộc." });
			}

			// Kiểm tra email đã tồn tại chưa
			var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
			if (existingUserByEmail != null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Email đã được sử dụng." });
			}

			// Kiểm tra username đã tồn tại chưa
			var existingUserByName = await userManager.FindByNameAsync(model.Username);
			if (existingUserByName != null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên đăng nhập đã được sử dụng." });
			}

			// Generate 6-digit OTP
			var otp = new Random().Next(100000, 999999).ToString();

			// Save to MemoryCache with 5 minutes expiration
			var cacheKey = $"signup_otp_{model.Email}";
			memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

			// Send Email
			var emailSent = await emailService.SendSignupOtpEmailAsync(model.Email, model.Username, otp);
			
			if (!emailSent)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Không thể gửi email OTP. Vui lòng thử lại sau." });
			}

			return IdentityResult.Success;
		}

		public async Task<IdentityResult> SignUpVerifyOtpAsync(SignUpVerifyOtpModel model)
		{
			if (string.IsNullOrEmpty(model.Otp))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Mã OTP là bắt buộc." });
			}

			var cacheKey = $"signup_otp_{model.Email}";
			if (memoryCache.TryGetValue(cacheKey, out string? cachedOtp))
			{
				if (cachedOtp == model.Otp)
				{
					// Valid OTP, remove from cache and proceed with signup
					memoryCache.Remove(cacheKey);
					return await SignUpAsync(model);
				}
				else
				{
					return IdentityResult.Failed(new IdentityError { Description = "Mã OTP không chính xác." });
				}
			}
			
			return IdentityResult.Failed(new IdentityError { Description = "Mã OTP đã hết hạn hoặc không tồn tại." });
		}

		public async Task<IdentityResult> SignUpAsync(SignUpModel model)
		{
			using var transaction = await context.Database.BeginTransactionAsync();
			try
			{
				if (string.IsNullOrEmpty(model.Password))
				{
					return IdentityResult.Failed(new IdentityError { Description = "Mật khẩu là bắt buộc." });
				}

				if (string.IsNullOrEmpty(model.Email))
				{
					return IdentityResult.Failed(new IdentityError { Description = "Email là bắt buộc." });
				}

				// Kiểm tra email đã tồn tại chưa
				var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
				if (existingUserByEmail != null)
				{
					return IdentityResult.Failed(new IdentityError { Description = "Email đã được sử dụng." });
				}

				// Kiểm tra username đã tồn tại chưa
				var existingUserByName = await userManager.FindByNameAsync(model.Username);
				if (existingUserByName != null)
				{
					return IdentityResult.Failed(new IdentityError { Description = "Tên đăng nhập đã được sử dụng." });
				}

				var user = new ApplicationUser
				{
					UserName = model.Username,
					Email = model.Email,
					LockoutEnabled = true
				};

				var result = await userManager.CreateAsync(user, model.Password);

				if (!result.Succeeded)
				{
					return result;
				}

				var userProfile = new UserProfile
				{
					UserId = user.Id,
					User = user
				};

				await context.UserProfiles!.AddAsync(userProfile);
				await context.SaveChangesAsync();

				if (!await roleManager.RoleExistsAsync(AppRole.Customer))
				{
					await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
				}

				await userManager.AddToRoleAsync(user, AppRole.Customer);

				// Generate welcome discount code
				await GenerateWelcomeDiscountCodeAsync(user.Id);

				// Xác nhận giao dịch thành công
				await transaction.CommitAsync();
				return result;
			}
			catch (Exception ex)
			{				
				// Rollback nếu có lỗi xảy ra
				await transaction.RollbackAsync();
				return IdentityResult.Failed(new IdentityError { Description = $"An error occurred: {ex.Message}" });
			}
		}

		public async Task<UserProfile?> GetUserProfileAsync(string userId)
		{
			return await context.UserProfiles!.Where(profile => profile.UserId == userId).FirstOrDefaultAsync();
		}

		public async Task<UserProfile?> SyncUserProfileFromAccountAsync(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null) return null;

			var profile = await context.UserProfiles!.Where(profile => profile.UserId == userId).FirstOrDefaultAsync();
			var hasChanges = false;

			if (profile == null)
			{
				profile = new UserProfile
				{
					UserId = userId,
					FullName = user.UserName ?? user.Email ?? string.Empty,
					Email = user.Email ?? string.Empty,
					PhoneNumber = user.PhoneNumber ?? string.Empty,
					User = user
				};

				await context.UserProfiles!.AddAsync(profile);
				hasChanges = true;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(profile.FullName))
				{
					profile.FullName = user.UserName ?? user.Email ?? string.Empty;
					hasChanges = true;
				}
				if (string.IsNullOrWhiteSpace(profile.Email) && !string.IsNullOrWhiteSpace(user.Email))
				{
					profile.Email = user.Email;
					hasChanges = true;
				}
				if (string.IsNullOrWhiteSpace(profile.PhoneNumber) && !string.IsNullOrWhiteSpace(user.PhoneNumber))
				{
					profile.PhoneNumber = user.PhoneNumber;
					hasChanges = true;
				}
			}

			if (hasChanges)
			{
				profile.UpdatedAt = DateTimeHelper.VietnamNow;
				await context.SaveChangesAsync();
			}

			return profile;
		}

		public async Task<List<string>?> GetRolesByUserIdAsync(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return null;
			}

			var roles = await userManager.GetRolesAsync(user);
			return roles.ToList();
		}

		public async Task<IdentityUser?> GetUserAccountAsync(string userId)
		{
			return await userManager.FindByIdAsync(userId);
		}

		public async Task<IEnumerable<IdentityUser>> GetAllUsersAsync()
		{
			return await context.Users.ToListAsync();
		}

		public async Task LogoutAsync(string userId)
		{
			// Xóa tất cả refresh tokens của user
			var tokens = await context.UserTokens
				.Where(t => t.UserId == userId && t.LoginProvider == "JWT" && t.Name == "RefreshToken")
				.ToListAsync();

			context.UserTokens.RemoveRange(tokens);
			await context.SaveChangesAsync();
		}

		private async Task<string> CreateAccessTokenAsync(ApplicationUser user)
		{
			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Name, user.UserName ?? ""),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var userRoles = await userManager.GetRolesAsync(user);
			foreach (var role in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, role));
			}

			var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.")));
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(authClaims),
				Expires = DateTime.UtcNow.AddHours(3), // 3 giờ
				NotBefore = DateTime.UtcNow,
				SigningCredentials = new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature),
				Issuer = configuration["Jwt:Issuer"],
				Audience = configuration["Jwt:Audience"]
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		private async Task GenerateWelcomeDiscountCodeAsync(string userId)
		{
			var discountCode = new DiscountCode
			{
				Code = GenerateRandomCode(8),
				DiscountType = "Percentage",
				DiscountPercentage = 10,
				DiscountAmount = 0,
				MaxDiscountAmount = 50000,
				MinOrderAmount = 0,
				ExpiryDate = DateTime.UtcNow.AddDays(30),
				IsUsed = false,
				UserId = userId,
				Quantity = 1,
				Status = "Active"
			};
			await context.DiscountCodes!.AddAsync(discountCode);
			await context.SaveChangesAsync();
		}

		private string GenerateRandomCode(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		private async Task<string> CreateAndSaveRefreshTokenAsync(string userId)
		{
			var refreshToken = Guid.NewGuid().ToString();
			var expiresAt = DateTime.UtcNow.AddDays(7);

			var existingTokens = await context.UserTokens
				.Where(t => t.UserId == userId
							&& t.LoginProvider == "JWT"
							&& t.Name == "RefreshToken")
				.ToListAsync();

			if (existingTokens.Any())
			{
				context.UserTokens.RemoveRange(existingTokens);
			}

			var userToken = new IdentityUserToken<string>
			{
				UserId = userId,
				LoginProvider = "JWT",
				Name = "RefreshToken",
				Value = refreshToken
			};

			await context.UserTokens.AddAsync(userToken);
			await context.SaveChangesAsync();

			return refreshToken;
		}

		public async Task<TokenResponseModel?> RefreshTokenAsync(string refreshToken)
		{
			// Tìm refresh token trong database
			var tokenRecord = await context.UserTokens
				.Where(t => t.LoginProvider == "JWT" 
					&& t.Name == "RefreshToken" 
					&& t.Value == refreshToken)
				.FirstOrDefaultAsync();

			if (tokenRecord == null)
			{
				return null; // Token không tồn tại
			}

			// Lấy user
			var user = await userManager.FindByIdAsync(tokenRecord.UserId);
			if (user == null || await userManager.IsLockedOutAsync(user))
			{
				// User không tồn tại hoặc bị khóa → Xóa token
				context.UserTokens.Remove(tokenRecord);
				await context.SaveChangesAsync();
				return null;
			}

			// Token rotation: Xóa refresh token cũ
			context.UserTokens.Remove(tokenRecord);

			// Tạo Access Token mới
			var newAccessToken = await CreateAccessTokenAsync(user);

			// Tạo Refresh Token mới
			var newRefreshToken = await CreateAndSaveRefreshTokenAsync(user.Id);

			await context.SaveChangesAsync();

			return new TokenResponseModel
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken,
				ExpiresIn = 10800, // 3 giờ
				TokenType = "Bearer"
			};
		}

		/// <summary>
		/// Revoke (hủy) Refresh Token
		/// </summary>
		public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? userId = null)
		{
			var query = context.UserTokens
				.Where(t => t.LoginProvider == "JWT" 
					&& t.Name == "RefreshToken" 
					&& t.Value == refreshToken);

			// Nếu có userId, chỉ revoke token của user đó
			if (!string.IsNullOrEmpty(userId))
			{
				query = query.Where(t => t.UserId == userId);
			}

			var tokenRecord = await query.FirstOrDefaultAsync();

			if (tokenRecord == null)
			{
				return false;
			}

			context.UserTokens.Remove(tokenRecord);
			await context.SaveChangesAsync();

			return true;
		}

		public async Task<List<ActiveSessionModel>> GetActiveSessionsAsync(string userId)
		{
			var tokens = await context.UserTokens
				.Where(t => t.UserId == userId 
					&& t.LoginProvider == "JWT" 
					&& t.Name == "RefreshToken")
				.OrderByDescending(t => t.UserId) // Sắp xếp theo thời gian tạo (cần thêm CreatedAt nếu có)
				.ToListAsync();

			// Note: IdentityUserToken không có CreatedAt, cần migration để thêm field này
			// Tạm thời dùng UserId để sort
			return tokens.Select(t => new ActiveSessionModel
			{
				RefreshToken = t.Value ?? "",
				CreatedAt = DateTime.UtcNow, // Tạm thời, cần thêm CreatedAt vào table
				ExpiresAt = DateTime.UtcNow.AddDays(7), // Tạm thời, cần lưu ExpiresAt
				IsCurrentSession = false // Cần logic để xác định session hiện tại
			}).ToList();
		}

		public async Task<string?> ForgotPasswordAsync(ForgotPasswordModel model)
		{
			try
			{
				// Tìm user bằng email hoặc username
				ApplicationUser? user = null;
				
				// Kiểm tra xem input có phải là email không
				if (model.EmailOrUsername.Contains("@"))
				{
					user = await userManager.FindByEmailAsync(model.EmailOrUsername);
				}
				else
				{
					user = await userManager.FindByNameAsync(model.EmailOrUsername);
				}

				if (user == null)
				{
					return "user_not_found";
				}

				if (string.IsNullOrEmpty(user.Email))
				{
					return "no_email";
				}

				// Kiểm tra tài khoản có bị khóa không
				if (await userManager.IsLockedOutAsync(user))
				{
					return "account_locked";
				}

				// Tạo mật khẩu mới (8 ký tự ngẫu nhiên)
				var newPassword = GenerateRandomPassword();

				// Reset password
				var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
				var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

				if (!result.Succeeded)
				{
					return "reset_failed";
				}

				// Gửi email
				var emailSent = await emailService.SendPasswordResetEmailAsync(
					user.Email, 
					user.UserName ?? user.Email, 
					newPassword
				);

				if (!emailSent)
				{
					return "email_failed";
				}

				return "success";
			}
			catch (Exception ex)
			{
				// Log error if needed
				Console.WriteLine($"ForgotPassword Error: {ex.Message}");
				return "error";
			}
		}

		public async Task<IdentityResult> UpdateUserRolesAsync(UpdateUserRolesModel model)
		{
			if (model.Roles == null || !model.Roles.Any())
			{
				return IdentityResult.Failed(new IdentityError { Description = "Danh sách quyền không được để trống." });
			}

			var user = await userManager.FindByIdAsync(model.UserId);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng." });
			}

			var normalizedRoles = model.Roles.Select(r => r.Trim())
				.Where(r => !string.IsNullOrWhiteSpace(r))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (normalizedRoles.Any(r => r.Length > 50))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên quyền quá dài (tối đa 50 ký tự)." });
			}

			// Tạo role nếu chưa tồn tại (không giới hạn danh sách cứng)
			foreach (var role in normalizedRoles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					var createResult = await roleManager.CreateAsync(new IdentityRole(role));
					if (!createResult.Succeeded)
					{
						return createResult;
					}
				}
			}

			var currentRoles = await userManager.GetRolesAsync(user);

			// Không được phép gỡ SuperAdmin khỏi user đang có nếu đã gán (bảo toàn super admin)
			var rolesToRemove = currentRoles
				.Where(r => !normalizedRoles.Contains(r, StringComparer.OrdinalIgnoreCase) && !string.Equals(r, AppRole.SuperAdmin, StringComparison.OrdinalIgnoreCase))
				.ToList();

			// Nếu user là super admin thì luôn giữ nguyên
			if (currentRoles.Contains(AppRole.SuperAdmin, StringComparer.OrdinalIgnoreCase) && !normalizedRoles.Contains(AppRole.SuperAdmin, StringComparer.OrdinalIgnoreCase))
			{
				normalizedRoles.Add(AppRole.SuperAdmin);
			}

			var rolesToAdd = normalizedRoles.Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

			if (rolesToRemove.Any())
			{
				var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
				if (!removeResult.Succeeded)
				{
					return removeResult;
				}
			}

			if (rolesToAdd.Any())
			{
				var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
				if (!addResult.Succeeded)
				{
					return addResult;
				}
			}

			return IdentityResult.Success;
		}

		public async Task<IEnumerable<string>> GetAvailableRolesAsync()
		{
			var roles = await roleManager.Roles
				.Where(r => r.Name != AppRole.SuperAdmin) // ẩn SuperAdmin
				.Select(r => r.Name)
				.ToListAsync();
			return roles.Where(r => !string.IsNullOrWhiteSpace(r))!;
		}

		public async Task<IdentityResult> AddRoleAsync(RoleCreateModel model)
		{
			var roleName = model.RoleName.Trim();
			if (string.IsNullOrWhiteSpace(roleName))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên quyền không được trống." });
			}

			if (roleName.Length > 50)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên quyền quá dài (tối đa 50 ký tự)." });
			}

			if (await roleManager.RoleExistsAsync(roleName))
			{
				// Không tạo mới nếu đã tồn tại, nhưng tránh trả lỗi
				return IdentityResult.Success;
			}

			return await roleManager.CreateAsync(new IdentityRole(roleName));
		}

		public Task<IEnumerable<string>> GetAvailableMenusAsync()
		{
			return Task.FromResult(AppMenu.All.AsEnumerable());
		}

		public async Task<IEnumerable<string>> GetMenusByRoleAsync(string roleName)
		{
			var role = await roleManager.FindByNameAsync(roleName);
			if (role == null) return Enumerable.Empty<string>();

			var claims = await roleManager.GetClaimsAsync(role);
			return claims
				.Where(c => c.Type == "menu")
				.Select(c => c.Value)
				.Where(v => !string.IsNullOrWhiteSpace(v))
				.ToList();
		}

		public async Task<IdentityResult> SetMenusForRoleAsync(string roleName, IEnumerable<string> menus)
		{
			var role = await roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy role." });
			}

			var normalizedMenus = menus
				.Where(m => !string.IsNullOrWhiteSpace(m))
				.Select(m => m.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (normalizedMenus.Any(m => m.Length > 50))
			{
				return IdentityResult.Failed(new IdentityError { Description = "Tên menu quá dài." });
			}

			// Filter only valid menus
			var validMenus = AppMenu.All;
			normalizedMenus = normalizedMenus.Where(m => validMenus.Contains(m)).ToList();

			// Current claims
			var currentClaims = await roleManager.GetClaimsAsync(role);
			var menuClaims = currentClaims.Where(c => c.Type == "menu").ToList();

			// remove ones not needed
			foreach (var claim in menuClaims)
			{
				if (!normalizedMenus.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))
				{
					await roleManager.RemoveClaimAsync(role, claim);
				}
			}

			// add new claims
			foreach (var menu in normalizedMenus)
			{
				if (!menuClaims.Any(c => string.Equals(c.Value, menu, StringComparison.OrdinalIgnoreCase)))
				{
					await roleManager.AddClaimAsync(role, new Claim("menu", menu));
				}
			}

			return IdentityResult.Success;
		}

		public async Task<IEnumerable<string>> GetMenusForUserAsync(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null) return Enumerable.Empty<string>();

			var roles = await userManager.GetRolesAsync(user);
			var menuSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var roleName in roles)
			{
				var menus = await GetMenusByRoleAsync(roleName);
				foreach (var menu in menus)
				{
					menuSet.Add(menu);
				}
			}

			return menuSet;
		}

		private string GenerateRandomPassword()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
			var random = new Random();
			var result = new char[8];
			
			// Đảm bảo có ít nhất 1 chữ hoa, 1 chữ thường, 1 số, 1 ký tự đặc biệt
			result[0] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)];
			result[1] = "abcdefghijklmnopqrstuvwxyz"[random.Next(26)];
			result[2] = "0123456789"[random.Next(10)];
			result[3] = "!@#$%"[random.Next(5)];
			
			// Fill còn lại với ký tự ngẫu nhiên
			for (int i = 4; i < 8; i++)
			{
				result[i] = chars[random.Next(chars.Length)];
			}
			
			// Shuffle array để randomize vị trí
			for (int i = result.Length - 1; i > 0; i--)
			{
				int j = random.Next(i + 1);
				(result[i], result[j]) = (result[j], result[i]);
			}
			
			return new string(result);
		}

		public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng." });
			}

			var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
			return result;
		}
	}
}
