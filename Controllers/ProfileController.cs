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

}