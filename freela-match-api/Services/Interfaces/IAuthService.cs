using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterDto dto);
        Task<User?> LoginAsync(string email, string password);
    }
}