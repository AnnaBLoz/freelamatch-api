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
    private readonly ProposalService _proposalService;

    public ReviewsController(ReviewsService reviewsService, UserService userService, ProposalService proposalService)
    {
        _reviewsService = reviewsService;
        _userService = userService;
        _proposalService = proposalService;
    }

    [HttpGet("")]
    public async Task<ActionResult<List<Reviews>>> GetReviews([FromQuery] int userId)
    {
        var reviews = await _reviewsService.GetReviews(userId);

        if (reviews == null)
            return NotFound(new { message = "Reviews not found" });

        return Ok(reviews);
    }

    [HttpGet("freelancer")]
    public async Task<ActionResult<List<Candidate>>> GetFreelancersToReview(int userId)
    {
        var proposal = await _proposalService.GetFreelancersToReview(userId);

        if (proposal == null)
            return NotFound(new { message = "Candidates not found" });

        return Ok(proposal);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReview([FromBody] ReviewCreate reviewCreate)
    {
        try
        {
            var review = await _reviewsService.CreateReview(reviewCreate);
            return Ok(new
            {
                review
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}