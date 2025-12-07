using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelaMatchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewsService _reviewsService;
        private readonly IUserService _userService;
        private readonly IProposalService _proposalService;

        public ReviewsController(IReviewsService reviewsService, IUserService userService, IProposalService proposalService)
        {
            _reviewsService = reviewsService;
            _userService = userService;
            _proposalService = proposalService;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<Reviews>>> GetReviews([FromQuery] int userId)
        {
            var reviews = await _reviewsService.GetReviews(userId);
            if (reviews == null || !reviews.Any())
                return NotFound(new { message = "Reviews not found" });

            return Ok(reviews);
        }

        [HttpGet("freelancer")]
        public async Task<ActionResult<List<Candidate>>> GetFreelancersToReview([FromQuery] int userId)
        {
            var candidates = await _proposalService.GetFreelancersToReview(userId);
            if (candidates == null || !candidates.Any())
                return NotFound(new { message = "Candidates not found" });

            return Ok(candidates);
        }

        [HttpGet("company")]
        public async Task<ActionResult<List<Proposal>>> GetCompaniesToReview([FromQuery] int userId)
        {
            var companies = await _proposalService.GetCompaniesToReview(userId);
            if (companies == null || !companies.Any())
                return NotFound(new { message = "Companies not found" });

            return Ok(companies);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreate reviewCreate)
        {
            try
            {
                var review = await _reviewsService.CreateReview(reviewCreate);
                return Ok(new { review });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
