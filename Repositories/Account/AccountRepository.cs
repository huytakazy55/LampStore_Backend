﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using LampStoreProjects.Data;
using LampStoreProjects.Helpers;
using LampStoreProjects.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LampStoreProjects.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly IConfiguration configuration;
		private readonly RoleManager<IdentityRole> roleManager;

		public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
			this.roleManager = roleManager;
		}

		public async Task<string> SignInAsync(SignInModel model)
		{
			var user = await userManager.FindByNameAsync(model.Username);
			var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);

			if (user == null || !passwordValid)
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(model.Username) || model.Password == null)
			{
				return string.Empty;
			}

			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, model.Username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var userEmail = await userManager.GetEmailAsync(user);
			authClaims.Add(new Claim(ClaimTypes.Email, userEmail.ToString()));

			var userRoles = await userManager.GetRolesAsync(user);
			foreach (var role in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
			}

			var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

			var token = new JwtSecurityToken(
				issuer: configuration["JWT:ValidIssuer"],
				audience: configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddMinutes(60),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task<IdentityResult> SignUpAsync(SignUpModel model)
		{
			var user = new ApplicationUser
			{
				FullName = model.FullName,
				UserName = model.Username,
				Email = model.Email,
				PhoneNumber = model.PhoneNumber
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
	}
}