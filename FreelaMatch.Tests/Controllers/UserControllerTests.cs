using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.FreelaMatch.Tests.Controllers
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

            _userServiceMock.Setup(s => s.GetUserByUserIdAsync(1))
                .ReturnsAsync(fakeUser);

            var result = await _controller.GetUser(1);

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;

            var data = ok!.Value.Should().BeAssignableTo<User>().Subject;
            data.Id.Should().Be(1);
            data.Name.Should().Be("Anna");
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userServiceMock.Setup(s => s.GetUserByUserIdAsync(2))
                .ReturnsAsync((User?)null);

            var result = await _controller.GetUser(2);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;

            var message = notFound!.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("User not found");
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
                Name = "Anna Updated",
                IsAvailable = false
            };

            _userServiceMock.Setup(s => s.UpdateUserAsync(1, updatedUser))
                .ReturnsAsync((true, "User updated successfully", fakeUser));

            var result = await _controller.UpdateUser(1, updatedUser);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;

            var data = ok!.Value.Should().BeAssignableTo<User>().Subject;
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

            var result = await _controller.UpdateUser(99, updatedUser);

            result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result as NotFoundObjectResult;

            var message = notFound!.Value.GetType().GetProperty("message")!.GetValue(notFound.Value);
            message.Should().Be("User not found");
        }
    }
}
