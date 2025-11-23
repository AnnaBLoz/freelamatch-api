using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelaMatchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GeneralController : ControllerBase
    {
        private readonly IGeneralService _generalService;
        private readonly IUserService _userService;

        public GeneralController(IGeneralService generalService, IUserService userService)
        {
            _generalService = generalService;
            _userService = userService;
        }

        [HttpGet("Freelancers")]
        public async Task<ActionResult<List<User>>> GetFreelancers()
        {
            var freelancers = await _generalService.GetFreelancers();

            if (freelancers == null || !freelancers.Any())
                return NotFound(new { message = "Freelancers not found" });

            return Ok(freelancers);
        }

        [HttpGet("Sectors")]
        public async Task<ActionResult<List<Sector>>> GetSectors()
        {
            var sectors = await _generalService.GetSectors();

            if (sectors == null || !sectors.Any())
                return NotFound(new { message = "Sectors not found" });

            return Ok(sectors);
        }

        [HttpGet("Skills")]
        public async Task<ActionResult<List<Skill>>> GetSkills()
        {
            var skills = await _generalService.GetSkills();

            if (skills == null || !skills.Any())
                return NotFound(new { message = "Skills not found" });

            return Ok(skills);
        }

        [HttpGet("CompletedProjects")]
        public async Task<ActionResult<List<Candidate>>> CompletedProjects([FromQuery] int userId)
        {
            var completed = await _generalService.CompletedProjects(userId);

            if (completed == null || !completed.Any())
                return NotFound(new { message = "Candidate not found" });

            return Ok(completed);
        }
    }
}
