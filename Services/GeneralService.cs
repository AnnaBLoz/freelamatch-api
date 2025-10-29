using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class GeneralService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public GeneralService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public Task<List<User?>> GetFreelancers()
    {
        return _context.Users
            .Where(f => f.Type == UserType.Freelancer)
            //.Include(f => f.Profile).ThenInclude(f => f.UserSkills).ThenInclude(f => f.Skill)
            .ToListAsync();
    }

    public Task<List<Sector?>> GetSectors()
    {
        return _context.Sector
            .ToListAsync();
    }
}
