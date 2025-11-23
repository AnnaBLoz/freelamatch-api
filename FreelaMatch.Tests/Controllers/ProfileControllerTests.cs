using Xunit;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FreelaMatch.Tests.Controllers
{
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _controller = new ProfileController(_profileServiceMock.Object);
        }

        [Fact]
        public async Task GetProfile_ShouldReturnOk_WhenProfileExists()
        {
            var profile = new Profile
            {
                UserId = 1,
                Biography = "Desenvolvedor experiente",
                ExperienceLevel = ExperienceLevel.Senior,
                PricePerHour = 150
            };

            _profileServiceMock
                .Setup(s => s.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            var result = await _controller.GetProfile(1);
            var ok = result.Result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(200, ok.StatusCode);
            Assert.Equal(profile, ok.Value);
        }

        [Fact]
        public async Task GetProfile_ShouldReturnNotFound_WhenNotExists()
        {
            _profileServiceMock
                .Setup(s => s.GetProfileByUserIdAsync(1))
                .ReturnsAsync((Profile?)null);

            var result = await _controller.GetProfile(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnOk_WhenSuccess()
        {
            var updateDto = new UpdateProfile
            {
                Biography = "Nova biografia",
                ExperienceLevel = ExperienceLevel.Pleno,
                PricePerHour = 120
            };

            var updated = new Profile
            {
                UserId = 1,
                Biography = updateDto.Biography,
                ExperienceLevel = updateDto.ExperienceLevel,
                PricePerHour = updateDto.PricePerHour
            };

            _profileServiceMock
                .Setup(s => s.UpdateProfileAsync(1, updateDto))
                .ReturnsAsync((true, "Updated", updated));

            var result = await _controller.UpdateProfile(1, updateDto);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(200, ok.StatusCode);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnNotFound_WhenFails()
        {
            var updateDto = new UpdateProfile
            {
                Biography = "Test",
                ExperienceLevel = ExperienceLevel.Junior,
                PricePerHour = 50
            };

            _profileServiceMock
                .Setup(s => s.UpdateProfileAsync(1, updateDto))
                .ReturnsAsync((false, "Not found", (Profile?)null));

            var result = await _controller.UpdateProfile(1, updateDto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSkills_ShouldReturnOk()
        {
            var skills = new List<Skill>
            {
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" }
            };

            _profileServiceMock
                .Setup(s => s.GetSkills())
                .ReturnsAsync(skills);

            var result = await _controller.GetSkills();
            var ok = result.Result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.Equal(skills, ok.Value);
        }

        [Fact]
        public async Task GetSkills_ShouldReturnNotFound_WhenEmpty()
        {
            _profileServiceMock
                .Setup(s => s.GetSkills())
                .ReturnsAsync(new List<Skill>());

            var result = await _controller.GetSkills();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}