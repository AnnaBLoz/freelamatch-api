using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using System.Threading.Tasks;

namespace FreelaMatchAPI.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUserIdAsync(int userId);
        Task<(bool Success, string Message, User? User)> UpdateUserAsync(int userId, UpdateUser updatedUser);
    }
}
