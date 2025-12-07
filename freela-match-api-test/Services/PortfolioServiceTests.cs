using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace freela_match_api_test.Services
{
    public class PortfolioServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // ---------------------------
        // GET PORTFOLIO BY USER
        // ---------------------------

        [Fact]
        public async Task GetPortfolioByUserIdAsync_ReturnsOnlyActivePortfolios()
        {
            // Arrange
            var context = GetDbContext();

            context.Portfolio.AddRange(
                new Portfolio { PortfolioId = 1, UserId = 5, URL = "url1", IsActive = true },
                new Portfolio { PortfolioId = 2, UserId = 5, URL = "url2", IsActive = false },
                new Portfolio { PortfolioId = 3, UserId = 10, URL = "url3", IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new PortfolioService(context);

            // Act
            var result = await service.GetPortfolioByUserIdAsync(5);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].PortfolioId);
            Assert.True(result[0].IsActive);
        }

        // ---------------------------
        // UPDATE PORTFOLIO
        // ---------------------------

        [Fact]
        public async Task UpdatePortfolioAsync_WhenPortfolioExists_UpdatesIt()
        {
            // Arrange
            var context = GetDbContext();

            context.Portfolio.Add(new Portfolio
            {
                PortfolioId = 1,
                URL = "old-url",
                IsActive = true,
                UserId = 10
            });
            await context.SaveChangesAsync();

            var service = new PortfolioService(context);

            var updated = new UpdatePortfolio
            {
                URL = "new-url",
                IsActive = false
            };

            // Act
            var result = await service.UpdatePortfolioAsync(1, updated);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Portfolio updated successfully", result.Message);
            Assert.NotNull(result.Portfolio);
            Assert.Equal("new-url", result.Portfolio!.URL);
            Assert.False(result.Portfolio.IsActive);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_WhenPortfolioDoesNotExist_ReturnsError()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PortfolioService(context);

            var updated = new UpdatePortfolio
            {
                URL = "new-url",
                IsActive = true
            };

            // Act
            var result = await service.UpdatePortfolioAsync(999, updated);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Portfolio not found", result.Message);
            Assert.Null(result.Portfolio);
        }

        // ---------------------------
        // CREATE PORTFOLIO
        // ---------------------------

        [Fact]
        public async Task CreatePortfolio_CreatesSuccessfully()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PortfolioService(context);

            var dto = new CreatePortfolio
            {
                URL = "created-url",
                IsActive = true,
                UserId = 77
            };

            // Act
            var result = await service.CreatePortfolio(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("created-url", result.URL);
            Assert.True(result.IsActive);
            Assert.Equal(77, result.UserId);
            Assert.True(result.PortfolioId > 0); // gerado pela memória
        }
    }
}
