using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ProfileService _profileService;

    public AuthController(AuthService authService, ProfileService profileService)
    {
        _authService = authService;
        _profileService = profileService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);

            await _profileService.CreateProfileAsync(user.Id, new UpdateProfile { });
            return Ok(new
            {
                user.Id,
                user.Email,
                user.Token,
                user.Type,
                user.Name
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _authService.LoginAsync(dto.Email, dto.Password);
        if (user == null) return Unauthorized("Email ou senha incorretos");

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Token,
            user.Type,
            user.Name,
            user.IsAvailable
        });
    }
}
