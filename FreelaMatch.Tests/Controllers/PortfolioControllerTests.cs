using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using System.Threading.Tasks;

namespace FreelaMatch.Tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioService> _service;
        private readonly PortfolioController _controller;

        public PortfolioControllerTests()
        {
            _service = new Mock<IPortfolioService>();
            _controller = new PortfolioController(_service.Object);
        }

        [Fact]
        public async Task CreatePortfolio_ReturnsOk()
        {
            var create = new CreatePortfolio
            {
                URL = "http://site.com",
                UserId = 1,
                IsActive = true
            };

            var saved = new Portfolio
            {
                PortfolioId = 1,
                URL = "http://site.com",
                UserId = 1,
                IsActive = true
            };

            _service.Setup(s => s.CreatePortfolioAsync(create))
                .ReturnsAsync(saved);

            var result = await _controller.CreatePortfolio(create);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Portfolio>(ok.Value);

            Assert.Equal("http://site.com", data.URL);
        }

        [Fact]
        public async Task UpdatePortfolio_ReturnsOk()
        {
            var update = new UpdatePortfolio
            {
                URL = "updated.com",
                IsActive = false
            };

            var updated = new Portfolio
            {
                PortfolioId = 1,
                URL = "updated.com",
                UserId = 1,
                IsActive = false
            };

            _service.Setup(s => s.UpdatePortfolioAsync(1, update))
                .ReturnsAsync(updated);

            var result = await _controller.UpdatePortfolio(1, update);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Portfolio>(ok.Value);

            Assert.Equal("updated.com", data.URL);
        }
    }
}
