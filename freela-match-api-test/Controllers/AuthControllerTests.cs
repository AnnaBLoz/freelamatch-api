using FluentAssertions;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace freela_match_api_test.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _profileServiceMock = new Mock<IProfileService>();

            _controller = new AuthController(
                _authServiceMock.Object,
                _profileServiceMock.Object
            );
        }

        // -------------------------------
        // REGISTER - SUCESSO
        // -------------------------------
        [Fact]
        public async Task Register_ShouldReturnOk_WhenUserIsCreated()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "123",
                Name = "Anna",
                Type = UserType.Freelancer
            };

            var fakeUser = new User
            {
                Id = 1,
                Email = dto.Email,
                Name = dto.Name,
                Token = "jwt-token",
                Type = dto.Type
            };

            _authServiceMock
                .Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync(fakeUser);

            _profileServiceMock
                .Setup(s => s.CreateProfileAsync(fakeUser.Id, It.IsAny<UpdateProfile>()))
                .ReturnsAsync((true, "Perfil criado", new Profile { UserId = fakeUser.Id }));

            // Act
            var result = await _controller.Register(dto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            dynamic data = result.Value!;
            ((int)data.Id).Should().Be(1);
            ((string)data.Email).Should().Be(dto.Email);
            ((string)data.Name).Should().Be(dto.Name);
        }

        // -------------------------------
        // REGISTER - FALHA (CORRIGIDO)
        // -------------------------------
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test", Password = "123", Type = UserType.Freelancer };

            _authServiceMock
                .Setup(s => s.RegisterAsync(dto))
                .ThrowsAsync(new InvalidOperationException("Email já cadastrado"));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            Assert.NotNull(badRequest);
            Assert.NotNull(badRequest.Value);

            // Usar reflexão para acessar propriedades de objetos anônimos
            var valueType = badRequest.Value.GetType();
            var messageProperty = valueType.GetProperty("message");
            Assert.NotNull(messageProperty);

            var message = messageProperty.GetValue(badRequest.Value) as string;
            Assert.Equal("Email já cadastrado", message);
        }

        // -------------------------------
        // LOGIN - SUCESSO
        // -------------------------------
        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "user@example.com", Password = "123" };

            var fakeUser = new User
            {
                Id = 10,
                Email = dto.Email,
                Name = "Anna",
                Token = "jwt-token",
                Type = UserType.Freelancer,
                IsAvailable = true
            };

            _authServiceMock
                .Setup(s => s.LoginAsync(dto.Email, dto.Password))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _controller.Login(dto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            dynamic data = result.Value!;
            ((string)data.Email).Should().Be(dto.Email);
            ((int)data.Id).Should().Be(fakeUser.Id);
        }

        // -------------------------------
        // LOGIN - FALHA
        // -------------------------------
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var dto = new LoginDto { Email = "wrong@example.com", Password = "wrong" };

            _authServiceMock
                .Setup(s => s.LoginAsync(dto.Email, dto.Password))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(dto) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(401);
            result.Value!.Should().Be("Email ou senha incorretos");
        }
    }
}