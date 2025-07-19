using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<UserProfileModel> GetByIdAsync(Guid id)
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
            UserProfile.UpdatedAt = DateTime.UtcNow;
            _context.UserProfiles!.Update(UserProfile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
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