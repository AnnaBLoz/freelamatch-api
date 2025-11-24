using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Xunit;

namespace freela_match_api_test
{
    public class AuthServiceTests
    {
        private AuthService CreateService(out AppDbContext context)
        {
            // DB em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = new AppDbContext(options);

            // Hasher real
            var passwordHasher = new PasswordHasher<User>();

            // IConfiguration fake (JWT)
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "12345678901234567890123456789012"}, // 32 bytes
                {"Jwt:Issuer", "FreelaMatch"},
                {"Jwt:Audience", "FreelaMatchUsers"},
                {"Jwt:ExpiresInMinutes", "30"}
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new AuthService(context, passwordHasher, config);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            var service = CreateService(out var context);

            var dto = new RegisterDto
            {
                Email = "teste@teste.com",
                Password = "123456",
                Name = "Anna",
                Type = UserType.Freelancer
            };

            var result = await service.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.False(string.IsNullOrEmpty(result.Password));
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.Equal(1, context.Users.Count());
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var service = CreateService(out var context);

            context.Users.Add(new User
            {
                Email = "anna@test.com",
                Password = "x",
                Token = "abc",
                Name = "Anna",
                Type = UserType.Freelancer
            });
            await context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                Email = "anna@test.com",
                Password = "123",
                Name = "Anna",
                Type = UserType.Freelancer
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenCredentialsAreValid()
        {
            var service = CreateService(out var context);
            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                Email = "anna@test.com",
                Name = "Anna",
                Type = UserType.Freelancer,
                Token = "initialtoken"
            };

            user.Password = hasher.HashPassword(user, "123456");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.LoginAsync("anna@test.com", "123456");

            Assert.NotNull(result);
            Assert.Equal("anna@test.com", result.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsWrong()
        {
            var service = CreateService(out var context);
            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                Email = "anna@test.com",
                Name = "Anna",
                Type = UserType.Freelancer,
                Token = "initialtoken"
            };

            user.Password = hasher.HashPassword(user, "123456");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.LoginAsync("anna@test.com", "wrongpassword");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            var service = CreateService(out var context);

            var result = await service.LoginAsync("notfound@test.com", "123");

            Assert.Null(result);
        }
    }
}
