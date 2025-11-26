using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Configuration;

namespace freela_match_api_test
{
    public class UserServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetConfigMock()
        {
            var mock = new Mock<IConfiguration>();
            return mock.Object;
        }

        // ---------------------------------------------------------
        // GET USER BY ID
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsUser_WithProfileAndSkills()
        {
            var context = GetDbContext();

            // SKILLS
            var skill1 = new Skill { SkillId = 1, Name = "C#" };
            var skill2 = new Skill { SkillId = 2, Name = "Angular" };

            context.Skills.AddRange(skill1, skill2);
            await context.SaveChangesAsync();

            // USER
            var user = new User
            {
                Id = 10,
                Name = "Anna",
                Email = "anna@test.com",
                Password = "hash123",
                Token = "token123",
                Profile = new Profile
                {
                    ProfileId = 5,
                    UserId = 10,
                    Biography = "Developer",
                    ExperienceLevel = ExperienceLevel.Junior,
                    PricePerHour = 120,
                    UserSkills = new List<UserSkill>
                    {
                        new UserSkill
                        {
                            UserId = 10,
                            ProfileId = 5,
                            SkillId = 1,
                            Skill = skill1,
                            IsActive = true
                        },
                        new UserSkill
                        {
                            UserId = 10,
                            ProfileId = 5,
                            SkillId = 2,
                            Skill = skill2,
                            IsActive = true
                        }
                    }
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Anna", result.Name);
            Assert.NotNull(result.Profile);
            Assert.Equal("Developer", result.Profile.Biography);
            Assert.Equal(2, result.Profile.UserSkills.Count);
            Assert.Equal("C#", result.Profile.UserSkills.First().Skill.Name);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var context = GetDbContext();
            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(999);

            Assert.Null(result);
        }

        // ---------------------------------------------------------
        // UPDATE USER
        // ---------------------------------------------------------
        [Fact]
        public async Task UpdateUserAsync_UpdatesUser_WhenUserExists()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 20,
                Name = "Old Name",
                Email = "old@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = false,
                Profile = new Profile
                {
                    ProfileId = 100,
                    UserId = 20,
                    Biography = "Old bio",
                    ExperienceLevel = ExperienceLevel.Junior,
                    PricePerHour = 50
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "New Name",
                IsAvailable = true
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(20, dto);

            Assert.True(success);
            Assert.Equal("User updated successfully", message);
            Assert.NotNull(updatedUser);
            Assert.Equal("New Name", updatedUser.Name);
            Assert.True(updatedUser.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsError_WhenUserNotFound()
        {
            var context = GetDbContext();
            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Testing",
                IsAvailable = false
            };

            var (success, message, user) = await service.UpdateUserAsync(999, dto);

            Assert.False(success);
            Assert.Equal("User not found", message);
            Assert.Null(user);
        }
    }
}
