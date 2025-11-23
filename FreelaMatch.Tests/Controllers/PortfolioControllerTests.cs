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
        private readonly Mock<IPortfolioService> _serviceMock;
        private readonly PortfolioController _controller;

        public PortfolioControllerTests()
        {
            _serviceMock = new Mock<IPortfolioService>();
            _controller = new PortfolioController(_serviceMock.Object);
        }

        // -----------------------------------------------------------------
        // GET PORTFOLIO - SUCESSO
        // -----------------------------------------------------------------
        [Fact]
        public async Task GetPortfolio_ShouldReturnOk_WhenPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                new Portfolio { PortfolioId = 1, UserId = 1, URL = "url1", IsActive = true },
                new Portfolio { PortfolioId = 2, UserId = 1, URL = "url2", IsActive = true }
            };

            _serviceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(portfolios);

            // Act
            var result = await _controller.GetPortfolio(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<Portfolio>>(ok.Value);
            data.Count.Should().Be(2);
        }

        // -----------------------------------------------------------------
        // GET PORTFOLIO - NOT FOUND
        // -----------------------------------------------------------------
        [Fact]
        public async Task GetPortfolio_ShouldReturnNotFound_WhenNoPortfolios()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetPortfolioByUserIdAsync(1))
                .ReturnsAsync(new List<Portfolio>());

            // Act
            var result = await _controller.GetPortfolio(1);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.StatusCode.Should().Be(404);

            var message = notFound.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("Portfolios not found");
        }

        // -----------------------------------------------------------------
        // UPDATE PORTFOLIO - SUCESSO
        // -----------------------------------------------------------------
        [Fact]
        public async Task UpdatePortfolio_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var updateDto = new UpdatePortfolio
            {
                URL = "updated-url",
                IsActive = true
            };

            var updatedPortfolio = new Portfolio
            {
                PortfolioId = 1,
                UserId = 1,
                URL = "updated-url",
                IsActive = true
            };

            _serviceMock
                .Setup(s => s.UpdatePortfolioAsync(1, updateDto))
                .ReturnsAsync((true, "Portfolio updated successfully", updatedPortfolio));

            // Act
            var result = await _controller.UpdatePortfolio(1, updateDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Portfolio>(ok.Value);
            data.URL.Should().Be("updated-url");
        }

        // -----------------------------------------------------------------
        // UPDATE PORTFOLIO - NOT FOUND
        // -----------------------------------------------------------------
        [Fact]
        public async Task UpdatePortfolio_ShouldReturnNotFound_WhenPortfolioNotFound()
        {
            // Arrange
            var updateDto = new UpdatePortfolio
            {
                URL = "new-url",
                IsActive = true
            };

            _serviceMock
                .Setup(s => s.UpdatePortfolioAsync(99, updateDto))
                .ReturnsAsync((false, "Portfolio not found", null));

            // Act
            var result = await _controller.UpdatePortfolio(99, updateDto);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.StatusCode.Should().Be(404);

            var message = notFound.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("Portfolio not found");
        }

        // -----------------------------------------------------------------
        // CREATE PORTFOLIO - SUCESSO
        // -----------------------------------------------------------------
        [Fact]
        public async Task Register_ShouldReturnOk_WhenPortfolioCreated()
        {
            // Arrange
            var createDto = new CreatePortfolio
            {
                UserId = 1,
                URL = "new-url",
                IsActive = true
            };

            var createdPortfolio = new Portfolio
            {
                PortfolioId = 1,
                UserId = 1,
                URL = "new-url",
                IsActive = true
            };

            _serviceMock
                .Setup(s => s.CreatePortfolio(createDto))
                .ReturnsAsync(createdPortfolio);

            // Act
            var result = await _controller.Register(createDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ((dynamic)ok.Value).portfolio;

            ((int)data.PortfolioId).Should().Be(1);
            ((string)data.URL).Should().Be("new-url");
        }
    }
}
