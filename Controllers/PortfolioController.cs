using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet("")]
    public async Task<ActionResult<List<Portfolio>>> GetPortfolios([FromQuery] int userId)
    {
        var portfolios = await _portfolioService.GetPortfolioByUserIdAsync(userId);

        if (portfolios == null || !portfolios.Any())
            return NotFound(new { message = "Portfolios not found" });

        return Ok(portfolios);
    }

    [HttpPut("{portfolioId}")]
    public async Task<IActionResult> UpdatePortfolio(int portfolioId, [FromBody] UpdatePortfolio updatedPortfolio)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _portfolioService.UpdatePortfolioAsync(portfolioId, updatedPortfolio);

        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.Portfolio);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Register([FromBody] CreatePortfolio portfolioCreate)
    {
        try
        {
            var portfolio = await _portfolioService.CreatePortfolio(portfolioCreate);
            return Ok(new { portfolio });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
