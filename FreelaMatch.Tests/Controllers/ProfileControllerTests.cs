using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreelaMatch.Tests.Controllers
{
    public class ProfileControllerTests
    {
        private readonly ProfileController _controller;
        private readonly AppDbContext _context;

        public ProfileControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ProfileTestsDb")
                .Options;

            _context = new AppDbContext(options);
            _controller = new ProfileController(_context);
        }

        [Fact]
        public async Task GetProfile_ReturnsOk_WhenProfileExists()
        {
            var profile = new Profile
            {
                ProfileId = 1,
                Biography = "Teste Bio",
                ExperienceLevel = ExperienceLevel.Junior,
                PricePerHour = 100,
                UserId = 1
            };

            _context.Profile.Add(profile);
            await _context.SaveChangesAsync();

            var result = await _controller.GetProfile(1);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok);

            var data = ok.Value as Profile;
            Assert.NotNull(data);
            Assert.Equal("Teste Bio", data.Biography);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsOk_WhenUpdatedSuccessfully()
        {
            var profile = new Profile
            {
                ProfileId = 1,
                Biography = "Old Bio",
                ExperienceLevel = ExperienceLevel.Junior,
                PricePerHour = 50,
                UserId = 1
            };

            _context.Profile.Add(profile);
            await _context.SaveChangesAsync();

            var update = new UpdateProfile
            {
                Biography = "New Bio",
                ExperienceLevel = ExperienceLevel.Senior,
                PricePerHour = 200,
                UserSkills = new List<UserSkill>()
            };

            var result = await _controller.UpdateProfile(1, update);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok);

            var updated = ok.Value as Profile;
            Assert.NotNull(updated);
            Assert.Equal("New Bio", updated.Biography);
        }
    }
}
