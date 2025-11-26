using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var dict = new Dictionary<string, string>();
            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
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
                UserId = 1,
                Biography = "Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Pleno
            };

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com",
                Password = "123",
                Token = "A",
                Profile = profile,
                UserSkills = new List<UserSkill>()
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetProfileByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Bio", result.Biography);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetProfileByUserIdAsync(999);

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
        }

        // ===============================
        // TESTE CreateProfileAsync
        // ===============================
        [Fact]
        public async Task CreateProfileAsync_ShouldCreateProfileSuccessfully()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com", // obrigatório
                Password = "123",          // obrigatório
                Token = "A",               // obrigatório
                Profile = null,
                UserSkills = new List<UserSkill>()
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());
            var updatedProfile = new UpdateProfile(); // vazio, será criado com valores padrão

            var (success, message, profile) = await service.CreateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("Perfil criado com sucesso.", message);
            Assert.NotNull(profile);
            Assert.Equal(1, profile.UserId);
        }

        [Fact]
        public async Task CreateProfileAsync_ShouldReturnFail_WhenProfileAlreadyExists()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1
            };

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com",
                Password = "123",
                Token = "A",
                Profile = profile,
                UserSkills = new List<UserSkill>()
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());
            var updatedProfile = new UpdateProfile();

            var (success, message, _) = await service.CreateProfileAsync(1, updatedProfile);

            Assert.False(success);
            Assert.Equal("Perfil já existe para este usuário.", message);
        }

        [Fact]
        public async Task CreateProfileAsync_ShouldReturnFail_WhenUserNotFound()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());
            var updatedProfile = new UpdateProfile();

            var (success, message, _) = await service.CreateProfileAsync(999, updatedProfile);

            Assert.False(success);
            Assert.Equal("Usuário não encontrado.", message);
        }

        // ===============================
        // TESTE UpdateProfileAsync
        // ===============================
        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateProfileAndSkillsSuccessfully()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Old Bio",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior
            };

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com",
                Password = "123",
                Token = "A",
                Profile = profile,
                UserSkills = new List<UserSkill>()
            };

            context.Users.Add(user);
            context.Skills.AddRange(
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" }
            );
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "New Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
                UserSkills = new List<UserSkill>
                {
                    new UserSkill { SkillId = 1 },
                    new UserSkill { SkillId = 2 }
                }
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("Profile updated successfully", message);
            Assert.NotNull(updated);
            Assert.Equal("New Bio", updated.Biography);
            Assert.Equal(100, updated.PricePerHour);
            Assert.Equal(ExperienceLevel.Senior, updated.ExperienceLevel);

            var userSkills = context.UserSkills.Where(us => us.UserId == 1 && us.IsActive).ToList();
            Assert.Equal(2, userSkills.Count);
            Assert.Contains(userSkills, us => us.SkillId == 1);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldReturnFail_WhenUserOrProfileNotFound()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio"
            };

            var (success, message, _) = await service.UpdateProfileAsync(999, updatedProfile);

            Assert.False(success);
            Assert.Equal("Profile not found", message);
        }
    }
}
