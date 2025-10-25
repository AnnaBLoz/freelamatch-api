using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class UserService
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
            .FirstOrDefaultAsync(p => p.Id == userId);
    }

    public async Task<(bool Success, string Message, User? User)> UpdateUserAsync(int userId, UpdateUser updatedUser)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        user.Name = updatedUser.Name;
        user.IsAvailable = updatedUser.IsAvailable;

        await _context.SaveChangesAsync();

        return (true, "User updated successfully", user);
    }

}
