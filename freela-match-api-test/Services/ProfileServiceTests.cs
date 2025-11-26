using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace freela_match_api_test.Services
{
    public class ProfileServiceTests
    {
        // ===============================
        // HELPER - BANCO IN MEMORY
        // ===============================
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetFakeConfig()
        {
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
        }

        // ===============================
        // TESTE GetProfileByUserIdAsync
        // ===============================
        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldReturnProfile_WhenExists()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 10,
                Biography = "Teste"
            };

            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetProfileByUserIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Teste", result.Biography);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetProfileByUserIdAsync(99);

            Assert.Null(result);
        }

        // ===============================
        // TESTE GetSkills
        // ===============================
        [Fact]
        public async Task GetSkills_ShouldReturnAllSkills()
        {
            var context = GetDbContext();

            context.Skills.AddRange(
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" }
            );

            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetSkills();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Name == "C#");
            Assert.Contains(result, s => s.Name == "Angular");
        }

        // ===============================
        // TESTE CreateProfileAsync
        // ===============================
        [Fact]
        public async Task CreateProfileAsync_ShouldReturnFail_WhenUserNotFound()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.CreateProfileAsync(1, new UpdateProfile());

            Assert.False(result.Success);
            Assert.Equal("Usuário não encontrado.", result.Message);
            Assert.Null(result.Profile);
        }

        [Fact]
        public async Task CreateProfileAsync_ShouldReturnFail_WhenProfileAlreadyExists()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com"
            };

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1
            };

            user.Profile = profile;
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.CreateProfileAsync(1, new UpdateProfile());

            Assert.False(result.Success);
            Assert.Equal("Perfil já existe para este usuário.", result.Message);
            Assert.Null(result.Profile);
        }

        [Fact]
        public async Task CreateProfileAsync_ShouldCreateProfileSuccessfully()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.CreateProfileAsync(1, new UpdateProfile());

            Assert.True(result.Success);
            Assert.Equal("Perfil criado com sucesso.", result.Message);
            Assert.NotNull(result.Profile);
            Assert.Equal(1, result.Profile.UserId);
        }

        // ===============================
        // TESTE UpdateProfileAsync
        // ===============================
        [Fact]
        public async Task UpdateProfileAsync_ShouldReturnFail_WhenUserOrProfileNotFound()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.UpdateProfileAsync(1, new UpdateProfile());

            Assert.False(result.Success);
            Assert.Equal("Profile not found", result.Message);
            Assert.Null(result.Profile);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateProfileAndSkillsSuccessfully()
        {
            var context = GetDbContext();

            var skill1 = new Skill { SkillId = 1, Name = "C#" };
            var skill2 = new Skill { SkillId = 2, Name = "Angular" };
            context.Skills.AddRange(skill1, skill2);

            var profile = new Profile { ProfileId = 1, UserId = 1, Biography = "Old", PricePerHour = 50, ExperienceLevel = ExperienceLevel.Junior };
            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Profile = profile,
                UserSkills = new List<UserSkill>
                {
                    new UserSkill { SkillId = 1, ProfileId = 1, UserId = 1, IsActive = true }
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "New Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
                UserSkills = new List<UserSkill>
                {
                    new UserSkill { SkillId = 2 }
                }
            };

            var result = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);
            Assert.NotNull(result.Profile);
            Assert.Equal("New Bio", result.Profile.Biography);
            Assert.Equal(100, result.Profile.PricePerHour);
            Assert.Equal(ExperienceLevel.Senior, result.Profile.ExperienceLevel);

            // Verifica as skills
            var activeSkills = user.UserSkills.Where(us => us.IsActive).Select(us => us.SkillId).ToList();
            Assert.Single(activeSkills);
            Assert.Contains(2, activeSkills);
        }
    }
}
