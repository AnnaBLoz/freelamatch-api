using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class GeneralController : ControllerBase
{
    private readonly GeneralService _generalService;
    private readonly UserService _userService;

    public GeneralController(GeneralService generalService, UserService userService)
    {
        _generalService = generalService;
        _userService = userService;
    }

    [HttpGet("Freelancers")]
    public async Task<ActionResult<List<User>>> GetFreelancers()
    {
        var freelancers = await _generalService.GetFreelancers();

        if (freelancers == null)
            return NotFound(new { message = "Freelancers not found" });

        return Ok(freelancers);
    }

    [HttpGet("Sectors")]
    public async Task<ActionResult<List<Sector>>> GetSectors()
    {
        var sectors = await _generalService.GetSectors();

        if (sectors == null)
            return NotFound(new { message = "Sectors not found" });

        return Ok(sectors);
    }
}