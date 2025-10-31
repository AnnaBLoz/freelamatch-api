using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewsService _reviewsService;
    private readonly UserService _userService;

    public ReviewsController(ReviewsService reviewsService, UserService userService)
    {
        _reviewsService = reviewsService;
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<ActionResult<List<Reviews>>> GetReviews([FromQuery] int userId)
    {
        var reviews = await _reviewsService.GetReviews(userId);

        if (reviews == null)
            return NotFound(new { message = "Reviews not found" });

        return Ok(reviews);
    }

}