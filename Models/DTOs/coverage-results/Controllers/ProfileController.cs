using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelaMatchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // ==========================================
        // GET PROFILE
        // ==========================================
        [HttpGet("")]
        public async Task<ActionResult<Profile>> GetProfile([FromQuery] int userId)
        {
            var profile = await _profileService.GetProfileByUserIdAsync(userId);

            // Se ainda não existe, criar automaticamente
            if (profile == null)
            {
                var created = await _profileService.CreateProfileAsync(userId, new UpdateProfile());

                if (!created.Success)
                    return NotFound(new { message = created.Message });

                profile = created.Profile;
            }

            return Ok(profile);
        }

        // ==========================================
        // UPDATE PROFILE
        // ==========================================
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

        // ==========================================
        // GET SKILLS
        // ==========================================
        [HttpGet("skills")]
        public async Task<ActionResult<List<Skill>>> GetSkills()
        {
            var skills = await _profileService.GetSkills();

            if (skills == null || !skills.Any())
                return NotFound(new { message = "Skills not found" });

            return Ok(skills);
        }
    }
}
