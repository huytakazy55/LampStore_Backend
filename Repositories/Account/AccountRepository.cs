using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using LampStoreProjects.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LampStoreProjects.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly IConfiguration configuration;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly ApplicationDbContext context;
		private readonly IEmailService emailService;

		public AccountRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IEmailService emailService)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.roleManager = roleManager;
			this.context = context;
			this.emailService = emailService;
		}

		public async Task<string?> SignInAsync(SignInModel model)
		{
			if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
			{
				return "blank";
			}

			var user = await userManager.FindByNameAsync(model.Username);
			if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
			{
				return null;
			}

			if (await userManager.IsLockedOutAsync(user))
			{
				return "lockout";
			}

			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Name, model.Username),
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

			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

			// Xóa tất cả các token cũ của người dùng trước khi thêm token mới
			var existingTokens = await context.UserTokens
				.Where(t => t.UserId == user.Id && t.LoginProvider == "JWT" && t.Name == "AccessToken")
				.ToListAsync();

			if (existingTokens.Any())
			{
				// Xóa tất cả các token cũ
				context.UserTokens.RemoveRange(existingTokens);
			}

			// Thêm token mới
			var userToken = new IdentityUserToken<string>
			{
				UserId = user.Id,
				LoginProvider = "JWT",
				Name = "AccessToken",
				Value = tokenString
			};

			await context.UserTokens.AddAsync(userToken);
			await context.SaveChangesAsync();

			return tokenString;
		}

		public async Task<string?> GoogleSignInAsync(GoogleSignInModel model)
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

				var token = tokenHandler.CreateToken(tokenDescriptor);
				var tokenString = tokenHandler.WriteToken(token);

				// Xóa token cũ và lưu token mới
				var existingTokens = await context.UserTokens
					.Where(t => t.UserId == user.Id && t.LoginProvider == "JWT" && t.Name == "AccessToken")
					.ToListAsync();

				if (existingTokens.Any())
				{
					context.UserTokens.RemoveRange(existingTokens);
				}

				var userToken = new IdentityUserToken<string>
				{
					UserId = user.Id,
					LoginProvider = "JWT",
					Name = "AccessToken",
					Value = tokenString
				};

				await context.UserTokens.AddAsync(userToken);
				await context.SaveChangesAsync();
				await transaction.CommitAsync();

				return tokenString;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				Console.WriteLine($"Google SignIn Error: {ex.Message}");
				return null;
			}
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
			var tokens = await context.UserTokens
				.Where(t => t.UserId == userId && t.LoginProvider == "JWT")
				.ToListAsync();

			context.UserTokens.RemoveRange(tokens);
			await context.SaveChangesAsync();
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
	}
}