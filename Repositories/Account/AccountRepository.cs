using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
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

		public AccountRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.roleManager = roleManager;
			this.context = context;
		}

		public async Task<string> SignInAsync(SignInModel model)
		{
			if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
			{
				return string.Empty;
			}

			var user = await userManager.FindByNameAsync(model.Username);
			if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
			{
				return string.Empty;
			}

			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.UserData, model.Username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var userRoles = await userManager.GetRolesAsync(user);
			foreach (var role in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, role));
			}

			var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(authClaims),
				Expires = DateTime.UtcNow.AddMinutes(60),
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

		public async Task<IdentityResult> SignUpAsync(SignUpModel model)
		{
			var user = new ApplicationUser
			{
				UserName = model.Username
			};

			var result = await userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				if (!await roleManager.RoleExistsAsync(AppRole.Customer))
				{
					await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
				}

				await userManager.AddToRoleAsync(user, AppRole.Customer);
			}
			return result;
		}

		public async Task<UserProfile> GetUserProfileAsync(string userId)
		{
			return await context.UserProfiles!.Where(profile => profile.UserId == userId).FirstOrDefaultAsync();
		}

		public async Task LogoutAsync(string userId)
		{
			var tokens = await context.UserTokens
				.Where(t => t.UserId == userId && t.LoginProvider == "JWT")
				.ToListAsync();

			context.UserTokens.RemoveRange(tokens);
			await context.SaveChangesAsync();
		}
	}
}