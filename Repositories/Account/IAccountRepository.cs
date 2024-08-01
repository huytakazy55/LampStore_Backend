using Microsoft.AspNetCore.Identity;
using LampStoreProjects.Models;

namespace LampStoreProjects.Repositories
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
    }
}