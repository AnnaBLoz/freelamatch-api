using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class ProfileService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;

    public ProfileService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<Profile?> GetProfileByUserIdAsync(int userId)
    {
        return await _context.Profiles
            .Include(p => p.UserSkills)
                .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
