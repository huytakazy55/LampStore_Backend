using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserProfileRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserProfileModel>> GetAllAsync()
        {
            var UserProfiles = await _context.UserProfiles!.ToListAsync();
            return _mapper.Map<IEnumerable<UserProfileModel>>(UserProfiles);
        }

        public async Task<UserProfileModel> GetByIdAsync(int id)
        {
            var UserProfile = await _context.UserProfiles!.FindAsync(id);
            return _mapper.Map<UserProfileModel>(UserProfile);
        }

        public async Task AddAsync(UserProfileModel UserProfileModel)
        {
            var UserProfile = _mapper.Map<UserProfile>(UserProfileModel);
            _context.UserProfiles!.Add(UserProfile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserProfileModel UserProfileModel)
        {
            var UserProfile = _mapper.Map<UserProfile>(UserProfileModel);
            _context.UserProfiles!.Update(UserProfile);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfileModel> GetProfileByUserIdAsync(string userName)
        {
            return await _context.UserProfiles!
                .Where(p => p.UserName == userName)
                .Select(p => new UserProfileModel
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.Andress,
                    ProfileAvatar = p.ProfileAvatar
                })
                .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var UserProfile = await _context.UserProfiles!.FindAsync(id);
            if (UserProfile != null)
            {
                _context.UserProfiles.Remove(UserProfile);
                await _context.SaveChangesAsync();
            }
        }
    }
}