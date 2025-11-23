using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FreelaMatchAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<User?> GetUserByUserIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserSkills).ThenInclude(u => u.Skill)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task<(bool Success, string Message, User? User)> UpdateUserAsync(int userId, UpdateUser updatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return (false, "User not found", null);

            user.Name = updatedUser.Name;
            user.IsAvailable = updatedUser.IsAvailable;

            await _context.SaveChangesAsync();

            return (true, "User updated successfully", user);
        }
    }
}
