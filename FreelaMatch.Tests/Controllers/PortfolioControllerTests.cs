using Xunit;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FreelaMatch.Tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly PortfolioController _controller;
        private readonly AppDbContext _context;

        public PortfolioControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("PortfolioTestsDb")
                .Options;

            _context = new AppDbContext(options);
            _controller = new PortfolioController(_context);
        }

        [Fact]
        public async Task CreatePortfolio_ReturnsOk()
        {
            var create = new CreatePortfolio
            {
                URL = "http://teste.com",
                IsActive = true,
                UserId = 1
            };

            var result = await _controller.CreatePortfolio(create);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok);

            var data = ok.Value as Portfolio;
            Assert.NotNull(data);
            Assert.Equal("http://teste.com", data.URL);
        }

        [Fact]
        public async Task UpdatePortfolio_ReturnsOk()
        {
            var portfolio = new Portfolio
            {
                PortfolioId = 1,
                URL = "old.com",
                IsActive = true,
                UserId = 1
            };

            _context.Portfolio.Add(portfolio);
            await _context.SaveChangesAsync();

            var update = new UpdatePortfolio
            {
                URL = "new.com",
                IsActive = false
            };

            var result = await _controller.UpdatePortfolio(1, update);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok);

            var updated = ok.Value as Portfolio;
            Assert.NotNull(updated);
            Assert.Equal("new.com", updated.URL);
            Assert.False(updated.IsActive);
        }
    }
}
