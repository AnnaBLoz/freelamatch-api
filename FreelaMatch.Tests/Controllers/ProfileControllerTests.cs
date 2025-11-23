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
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _controller = new ProfileController(_profileServiceMock.Object);
        }

        // ------------------------------------------------------------
        // GET PROFILE
        // ------------------------------------------------------------
        [Fact]
        public async Task GetProfile_ShouldReturnOk_WhenProfileExists()
        {
            var fakeProfile = new Profile
            {
                Id = 1,
                Bio = "Test Bio",
                UserId = 1
            };

            _profileServiceMock.Setup(s => s.GetProfileByUserIdAsync(1))
                .ReturnsAsync(fakeProfile);

            var result = await _controller.GetProfile(1);

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;

            var data = ok!.Value.Should().BeAssignableTo<Profile>().Subject;
            data.Id.Should().Be(1);
            data.Bio.Should().Be("Test Bio");
        }

        [Fact]
        public async Task GetProfile_ShouldReturnNotFound_WhenProfileDoesNotExist()
        {
            _profileServiceMock.Setup(s => s.GetProfileByUserIdAsync(1))
                .ReturnsAsync((Profile?)null);

            var result = await _controller.GetProfile(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;

            var message = notFound!.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("Profile not found");
        }

        // ------------------------------------------------------------
        // UPDATE PROFILE
        // ------------------------------------------------------------
        [Fact]
        public async Task UpdateProfile_ShouldReturnOk_WhenUpdateSuccess()
        {
            var updated = new UpdateProfile { Bio = "Updated Bio" };

            var fakeProfile = new Profile
            {
                Id = 1,
                Bio = "Updated Bio",
                UserId = 1
            };

            _profileServiceMock.Setup(s => s.UpdateProfileAsync(1, updated))
                .ReturnsAsync((true, "Profile updated successfully", fakeProfile));

            var result = await _controller.UpdateProfile(1, updated);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;

            var data = ok!.Value.Should().BeAssignableTo<Profile>().Subject;
            data.Bio.Should().Be("Updated Bio");
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnNotFound_WhenProfileDoesNotExist()
        {
            var updated = new UpdateProfile { Bio = "Updated Bio" };

            _profileServiceMock.Setup(s => s.UpdateProfileAsync(99, updated))
                .ReturnsAsync((false, "Profile not found", null));

            var result = await _controller.UpdateProfile(99, updated);

            result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result as NotFoundObjectResult;

            var message = notFound!.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("Profile not found");
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Bio", "Required");

            var updated = new UpdateProfile { Bio = "" };

            var result = await _controller.UpdateProfile(1, updated);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ------------------------------------------------------------
        // GET SKILLS
        // ------------------------------------------------------------
        [Fact]
        public async Task GetSkills_ShouldReturnOk_WhenSkillsExist()
        {
            var skills = new List<Skill>
            {
                new Skill { SkillId = 1, SkillName = "C#" },
                new Skill { SkillId = 2, SkillName = "Angular" }
            };

            _profileServiceMock.Setup(s => s.GetSkills())
                .ReturnsAsync(skills);

            var result = await _controller.GetSkills();

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;

            var data = ok!.Value.Should().BeAssignableTo<List<Skill>>().Subject;
            data.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetSkills_ShouldReturnNotFound_WhenNoSkills()
        {
            _profileServiceMock.Setup(s => s.GetSkills())
                .ReturnsAsync(new List<Skill>());

            var result = await _controller.GetSkills();

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;

            var message = notFound!.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("Skills not found");
        }
    }
}
