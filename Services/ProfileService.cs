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
    private readonly IConfiguration _config;

    public ProfileService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<Profile?> GetProfileByUserIdAsync(int userId)
    {
        return await _context.Profiles
            .Include(p => p.UserSkills.Where(us => us.IsActive))
            .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<List<Skill>> GetSkills()
    {
        return await _context.Skills.ToListAsync();
    }

    public async Task<(bool Success, string Message, Profile? Profile)> UpdateProfileAsync(int userId, UpdateProfile updatedProfile)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserSkills)
            .ThenInclude(u => u.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.Profile == null)
            return (false, "Profile not found", null);

        var profile = user.Profile;

        profile.Biography = updatedProfile.Biography;
        profile.PricePerHour = updatedProfile.PricePerHour;
        profile.ExperienceLevel = updatedProfile.ExperienceLevel;

        var updatedSkillIds = updatedProfile.UserSkills?.Select(us => us.SkillId).ToList() ?? new List<int>();

        foreach (var skillId in updatedSkillIds)
        {
            var existingUserSkill = user.UserSkills.FirstOrDefault(us => us.SkillId == skillId);

            if (existingUserSkill != null)
            {
                if (!existingUserSkill.IsActive)
                {
                    existingUserSkill.IsActive = true;
                }
            }
            else
            {
                _context.UserSkills.Add(new UserSkill
                {
                    UserId = userId,
                    SkillId = skillId,
                    ProfileId = user.Profile.ProfileId,
                    IsActive = true
                });
            }
        }

        foreach (var userSkill in user.UserSkills)
        {
            if (!updatedSkillIds.Contains(userSkill.SkillId) && userSkill.IsActive)
            {
                userSkill.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();

        return (true, "Profile updated successfully", profile);
    }

}
