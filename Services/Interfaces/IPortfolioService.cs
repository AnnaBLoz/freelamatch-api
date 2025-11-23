using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Interfaces
{
    public interface IPortfolioService
    {
        Task<List<Portfolio>> GetPortfolioByUserIdAsync(int userId);
        Task<(bool Success, string Message, Portfolio? Portfolio)> UpdatePortfolioAsync(int portfolioId, UpdatePortfolio updatedPortfolio);
        Task<Portfolio> CreatePortfolio(CreatePortfolio portfolioCreated);
    }
}
