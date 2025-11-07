using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<ActionResult<User>> GetUser([FromQuery] int userId)
    {
        var user = await _userService.GetUserByUserIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUser updatedUser)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.UpdateUserAsync(userId, updatedUser);

        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.User);
    }

}