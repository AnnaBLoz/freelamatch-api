using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioService> _portfolioServiceMock;
        private readonly PortfolioController _controller;

        public PortfolioControllerTests()
        {
            _portfolioServiceMock = new Mock<IPortfolioService>();
            _controller = new PortfolioController(_portfolioServiceMock.Object);
        }

        // -------------------------------
        // GET PORTFOLIOS - SUCESSO
        // -------------------------------
        [Fact]
        public async Task GetPortfolios_ShouldReturnOk_WhenPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                new Portfolio { PortfolioId = 1, UserId = 1, URL = "url1", IsActive = true },
                new Portfolio { PortfolioId = 2, UserId = 1, URL = "url2", IsActive = true }
            };

            _portfolioServiceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(portfolios);

            // Act
            var actionResult = await _controller.GetPortfolios(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<Portfolio>>(okResult.Value);
            data.Count.Should().Be(2);
        }

        // -------------------------------
        // GET PORTFOLIOS - FALHA
        // -------------------------------
        [Fact]
        public async Task GetPortfolios_ShouldReturnNotFound_WhenNoPortfolios()
        {
            // Arrange
            _portfolioServiceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(new List<Portfolio>());

            // Act
            var actionResult = await _controller.GetPortfolios(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            notFoundResult.StatusCode.Should().Be(404);

            var message = notFoundResult.Value.GetType().GetProperty("message")!.GetValue(notFoundResult.Value);
            message.Should().Be("Portfolios not found");
        }

        // -------------------------------
        // UPDATE PORTFOLIO - SUCESSO
        // -------------------------------
        [Fact]
        public async Task UpdatePortfolio_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var updatedPortfolio = new UpdatePortfolio { URL = "new-url", IsActive = true };
            var portfolio = new Portfolio { PortfolioId = 1, URL = "new-url", IsActive = true };

            _portfolioServiceMock
                .Setup(s => s.UpdatePortfolioAsync(1, updatedPortfolio))
                .ReturnsAsync((true, "Portfolio updated successfully", portfolio));

            // Act
            var actionResult = await _controller.UpdatePortfolio(1, updatedPortfolio);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var data = Assert.IsType<Portfolio>(okResult.Value);
            data.URL.Should().Be("new-url");
        }

        // -------------------------------
        // UPDATE PORTFOLIO - FALHA
        // -------------------------------
        [Fact]
        public async Task UpdatePortfolio_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var updatedPortfolio = new UpdatePortfolio { URL = "new-url", IsActive = true };

            _portfolioServiceMock
                .Setup(s => s.UpdatePortfolioAsync(99, updatedPortfolio))
                .ReturnsAsync((false, "Portfolio not found", null));

            // Act
            var actionResult = await _controller.UpdatePortfolio(99, updatedPortfolio);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            notFoundResult.StatusCode.Should().Be(404);

            var message = notFoundResult.Value.GetType().GetProperty("message")!.GetValue(notFoundResult.Value);
            message.Should().Be("Portfolio not found");
        }

        // -------------------------------
        // CREATE PORTFOLIO - SUCESSO
        // -------------------------------
        [Fact]
        public async Task Register_ShouldReturnOk_WhenPortfolioCreated()
        {
            // Arrange
            var createDto = new CreatePortfolio { UserId = 1, URL = "url", IsActive = true };
            var portfolio = new Portfolio { PortfolioId = 1, UserId = 1, URL = "url", IsActive = true };

            _portfolioServiceMock
                .Setup(s => s.CreatePortfolio(createDto))
                .ReturnsAsync(portfolio);

            // Act
            var actionResult = await _controller.Register(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var data = ((dynamic)okResult.Value).portfolio;
            ((int)data.PortfolioId).Should().Be(1);
        }
    }
}
