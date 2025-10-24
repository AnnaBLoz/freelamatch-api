using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("")]
    public async Task<ActionResult<Profile>> GetProfile([FromQuery] int userId)
    {
        var profile = await _profileService.GetProfileByUserIdAsync(userId);

        if (profile == null)
            return NotFound(new { message = "Profile not found" });

        return Ok(profile);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateProfile updatedProfile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _profileService.UpdateProfileAsync(userId, updatedProfile);

        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.Profile);
    }

    [HttpGet("skills")]
    public async Task<ActionResult<List<Skill>>> GetSkills()
    {
        var skills = await _profileService.GetSkills();

        if (skills == null || !skills.Any())
            return NotFound(new { message = "Skills not found" });

        return Ok(skills);
    }

}