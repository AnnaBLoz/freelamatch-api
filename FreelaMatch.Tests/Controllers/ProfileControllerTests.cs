using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreelaMatch.Tests.Controllers
{
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileService> _service;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _service = new Mock<IProfileService>();
            _controller = new ProfileController(_service.Object);
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

            _service.Setup(s => s.GetProfileAsync(1))
                .ReturnsAsync(profile);

            var result = await _controller.GetProfile(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Profile>(ok.Value);

            Assert.Equal("Teste Bio", data.Biography);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsOk()
        {
            var update = new UpdateProfile
            {
                Biography = "Updated",
                ExperienceLevel = ExperienceLevel.Senior,
                PricePerHour = 250
            };

            var updated = new Profile
            {
                ProfileId = 1,
                Biography = "Updated",
                ExperienceLevel = ExperienceLevel.Senior,
                PricePerHour = 250
            };

            _service.Setup(s => s.UpdateProfileAsync(1, update))
                .ReturnsAsync(updated);

            var result = await _controller.UpdateProfile(1, update);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Profile>(ok.Value);

            Assert.Equal("Updated", data.Biography);
        }
    }
}
