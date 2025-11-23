using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreelaMatchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        // ==========================================
        // GET /api/portfolio/{userId}
        // ==========================================
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<Portfolio>>> GetPortfolio(int userId)
        {
            var portfolios = await _portfolioService.GetPortfolioByUserIdAsync(userId);

            if (portfolios == null || portfolios.Count == 0)
                return NotFound(new { message = "Portfolios not found" });

            return Ok(portfolios);
        }

        // ==========================================
        // POST /api/portfolio
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolio createPortfolio)
        {
            var portfolio = await _portfolioService.CreatePortfolio(createPortfolio);

            if (portfolio == null)
                return BadRequest(new { message = "Failed to create portfolio" });

            return Ok(portfolio);
        }

        // ==========================================
        // PUT /api/portfolio/{id}
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] UpdatePortfolio updatePortfolio)
        {
            var result = await _portfolioService.UpdatePortfolioAsync(id, updatePortfolio);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(result.Portfolio);
        }
    }
}