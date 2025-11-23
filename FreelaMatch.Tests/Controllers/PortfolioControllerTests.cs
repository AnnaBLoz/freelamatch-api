using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FreelaMatch.Tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioService> _serviceMock;
        private readonly PortfolioController _controller;

        public PortfolioControllerTests()
        {
            _serviceMock = new Mock<IPortfolioService>();
            _controller = new PortfolioController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetPortfolio_ShouldReturnOk_WhenDataExists()
        {
            var portfolios = new List<Portfolio>
            {
                new Portfolio { Id = 1, UserId = 1, Title = "Proj 1" }
            };

            _serviceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(portfolios);

            var result = await _controller.GetPortfolio(1);
            var ok = result.Result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(portfolios, ok.Value);
        }

        [Fact]
        public async Task GetPortfolio_ShouldReturnNotFound_WhenEmpty()
        {
            _serviceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(new List<Portfolio>());

            var result = await _controller.GetPortfolio(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnOk_WhenSuccess()
        {
            var updateDto = new UpdatePortfolio
            {
                Description = "Editado",
                Title = "Novo título"
            };

            var updated = new Portfolio
            {
                Id = 1,
                UserId = 1,
                Title = updateDto.Title,
                Description = updateDto.Description
            };

            _serviceMock
                .Setup(s => s.UpdatePortfolioAsync(1, updateDto))
                .ReturnsAsync((true, "Updated", updated));

            var result = await _controller.UpdatePortfolio(1, updateDto);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnNotFound_WhenFails()
        {
            var updateDto = new UpdatePortfolio
            {
                Description = "Teste",
                Title = "X"
            };

            _serviceMock
                .Setup(s => s.UpdatePortfolioAsync(1, updateDto))
                .ReturnsAsync((false, "Not found", (Portfolio?)null));

            var result = await _controller.UpdatePortfolio(1, updateDto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreatePortfolio_ShouldReturnOk()
        {
            var createDto = new CreatePortfolio
            {
                Description = "Desc",
                Title = "Título",
                UserId = 1
            };

            var created = new Portfolio
            {
                Id = 1,
                UserId = createDto.UserId,
                Title = createDto.Title,
                Description = createDto.Description
            };

            _serviceMock
                .Setup(s => s.CreatePortfolio(createDto))
                .ReturnsAsync(created);

            var result = await _controller.CreatePortfolio(createDto);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(created, ok.Value);
        }
    }
}
