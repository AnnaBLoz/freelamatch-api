using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
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
        // GET /api/portfolio/{freelancerId}
        // ==========================================
        [HttpGet("{freelancerId}")]
        public async Task<IActionResult> GetPortfolio(int freelancerId)
        {
            var portfolio = await _portfolioService.GetPortfolioAsync(freelancerId);

            if (portfolio == null)
                return NotFound(new { message = "Portfolios not found" });

            return Ok(portfolio);
        }

        // ==========================================
        // POST /api/portfolio
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreatePortfolio([FromBody] Portfolio portfolio)
        {
            var result = await _portfolioService.CreatePortfolioAsync(portfolio);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { portfolio = result.Portfolio });
        }

        // ==========================================
        // PUT /api/portfolio/{id}
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] Portfolio portfolio)
        {
            var result = await _portfolioService.UpdatePortfolioAsync(id, portfolio);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Portfolio);
        }
    }
}
