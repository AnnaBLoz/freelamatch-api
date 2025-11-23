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
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task GetUser_ShouldReturnOk_WhenUserExists()
        {
            var fakeUser = new User
            {
                Id = 1,
                Name = "Anna",
                Email = "test@example.com",
                Type = UserType.Freelancer,
                IsAvailable = true
            };

            _userServiceMock.Setup(s => s.GetUserByUserIdAsync(1)).ReturnsAsync(fakeUser);

            var actionResult = await _controller.GetUser(1);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            okResult.StatusCode.Should().Be(200);
            var data = Assert.IsType<User>(okResult.Value);
            data.Id.Should().Be(1);
            data.Name.Should().Be("Anna");
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userServiceMock.Setup(s => s.GetUserByUserIdAsync(2)).ReturnsAsync((User?)null);

            var actionResult = await _controller.GetUser(2);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);

            notFoundResult.StatusCode.Should().Be(404);
            var message = notFoundResult.Value.GetType().GetProperty("message")!.GetValue(notFoundResult.Value);
            Assert.Equal("User not found", message);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var updatedUser = new UpdateUser
            {
                Name = "Anna Updated",
                IsAvailable = false
            };

            var fakeUser = new User
            {
                Id = 1,
                Name = updatedUser.Name,
                IsAvailable = updatedUser.IsAvailable
            };

            _userServiceMock.Setup(s => s.UpdateUserAsync(1, updatedUser))
                .ReturnsAsync((true, "User updated successfully", fakeUser));

            var actionResult = await _controller.UpdateUser(1, updatedUser);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);

            okResult.StatusCode.Should().Be(200);
            var data = Assert.IsType<User>(okResult.Value);
            data.Id.Should().Be(1);
            data.Name.Should().Be("Anna Updated");
            data.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var updatedUser = new UpdateUser
            {
                Name = "Non-existent",
                IsAvailable = true
            };

            _userServiceMock.Setup(s => s.UpdateUserAsync(99, updatedUser))
                .ReturnsAsync((false, "User not found", null));

            var actionResult = await _controller.UpdateUser(99, updatedUser);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);

            notFoundResult.StatusCode.Should().Be(404);
            var message = notFoundResult.Value.GetType().GetProperty("message")!.GetValue(notFoundResult.Value);
            Assert.Equal("User not found", message);
        }
    }
}
