using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;
using System.Security.Claims;
using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<TokenResponseModel?> SignInAsync(SignInModel model);
        Task<TokenResponseModel?> GoogleSignInAsync(GoogleSignInModel model);
        Task<TokenResponseModel?> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? userId = null);
        Task<List<ActiveSessionModel>> GetActiveSessionsAsync(string userId);

        Task<UserProfile?> GetUserProfileAsync(string userId);
        Task<IdentityUser?> GetUserAccountAsync(string userId);
        Task<IEnumerable<IdentityUser>> GetAllUsersAsync();
        Task<List<string>?> GetRolesByUserIdAsync(string userId);
        Task LogoutAsync(string userId);
        Task<string?> ForgotPasswordAsync(ForgotPasswordModel model);
        Task<IdentityResult> UpdateUserRolesAsync(UpdateUserRolesModel model);
        Task<IEnumerable<string>> GetAvailableRolesAsync();
        Task<IdentityResult> AddRoleAsync(RoleCreateModel model);
        Task<IEnumerable<string>> GetAvailableMenusAsync();
        Task<IEnumerable<string>> GetMenusByRoleAsync(string roleName);
        Task<IdentityResult> SetMenusForRoleAsync(string roleName, IEnumerable<string> menus);
        Task<IEnumerable<string>> GetMenusForUserAsync(string userId);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    }
}