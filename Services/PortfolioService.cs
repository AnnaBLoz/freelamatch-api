using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class PortfolioService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public PortfolioService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<List<Portfolio>> GetPortfolioByUserIdAsync(int userId)
    {
        return await _context.Portfolio
            .Where(p => p.UserId == userId && p.IsActive == true)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, Portfolio? Portfolio)> UpdatePortfolioAsync(int portfolioId, UpdatePortfolio updatedPortfolio)
    {
        var portfolio = await _context.Portfolio
            .FirstOrDefaultAsync(u => u.PortfolioId == portfolioId);

        portfolio.URL = updatedPortfolio.URL;
        portfolio.IsActive = updatedPortfolio.IsActive;

        await _context.SaveChangesAsync();

        return (true, "Portfolio updated successfully", portfolio);
    }

    public async Task<Portfolio> CreatePortfolio(CreatePortfolio portfolioCreated)
    {

        var portfolio = new Portfolio
        {
            URL = portfolioCreated.URL,
            IsActive = portfolioCreated.IsActive,
            UserId = portfolioCreated.UserId
        };

        _context.Portfolio.Add(portfolio);
        await _context.SaveChangesAsync();

        return portfolio;
    }

}
