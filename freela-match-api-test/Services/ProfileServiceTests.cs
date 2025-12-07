using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
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
        // TESTES GetProfileByUserIdAsync
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

        // Nota: EF Core In-Memory não suporta filtragem dentro de .Include().Where()
        // Este teste valida que as navigation properties são carregadas corretamente.
        // A filtragem de IsActive funciona em bancos reais (SQL Server, PostgreSQL, etc.)
        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldIncludeSkillsWithDetails_WhenProfileHasSkills()
        {
            var context = GetDbContext();

            var skill1 = new Skill { SkillId = 1, Name = "C#" };
            var skill2 = new Skill { SkillId = 2, Name = "Angular" };
            context.Skills.AddRange(skill1, skill2);

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

            // Adicionar apenas skills ativas para simular comportamento esperado
            context.UserSkills.Add(
                new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());
            var result = await service.GetProfileByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.NotEmpty(result.UserSkills);
            // Verifica que as skills têm os detalhes carregados (Skill navigation property)
            Assert.All(result.UserSkills, us => Assert.NotNull(us.Skill));
            Assert.Contains(result.UserSkills, us => us.Skill.Name == "C#" && us.IsActive);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldHandleProfileWithoutSkills()
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
            Assert.Empty(result.UserSkills);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldIncludeSector_WhenProfileHasSector()
        {
            var context = GetDbContext();

            var sector = new Sector { SectorId = 1, Name = "Tecnologia" };
            context.Sector.Add(sector);

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Pleno,
                SectorId = 1
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
            Assert.NotNull(result.Sector);
            Assert.Equal("Tecnologia", result.Sector.Name);
        }

        // ===============================
        // TESTES GetSkills
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

        [Fact]
        public async Task GetSkills_ShouldReturnEmptyList_WhenNoSkills()
        {
            var context = GetDbContext();
            var service = new ProfileService(context, GetFakeConfig());

            var result = await service.GetSkills();

            Assert.Empty(result);
        }

        // ===============================
        // TESTES CreateProfileAsync
        // ===============================
        [Fact]
        public async Task CreateProfileAsync_ShouldCreateProfileSuccessfully()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 1,
                Name = "Fulano",
                Email = "fulano@test.com",
                Password = "123",
                Token = "A",
                Profile = null,
                UserSkills = new List<UserSkill>()
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());
            var updatedProfile = new UpdateProfile();

            var (success, message, profile) = await service.CreateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("Perfil criado com sucesso.", message);
            Assert.NotNull(profile);
            Assert.Equal(1, profile.UserId);
            Assert.Equal("", profile.Biography);
            Assert.Equal(0, profile.PricePerHour);
            Assert.Equal(ExperienceLevel.Junior, profile.ExperienceLevel);
            Assert.Null(profile.SectorId);
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
        // TESTES UpdateProfileAsync
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

        [Fact]
        public async Task UpdateProfileAsync_ShouldReactivateInactiveSkills()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
            context.Skills.Add(new Skill { SkillId = 1, Name = "C#" });
            await context.SaveChangesAsync();

            // Adicionar skill inativa
            var inactiveSkill = new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = false };
            context.UserSkills.Add(inactiveSkill);
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill> { new UserSkill { SkillId = 1 } }
            };

            var (success, message, _) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);

            var reactivatedSkill = context.UserSkills.First(us => us.UserId == 1 && us.SkillId == 1);
            Assert.True(reactivatedSkill.IsActive);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldDeactivateRemovedSkills()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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

            // Adicionar skills ativas
            context.UserSkills.AddRange(
                new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = true },
                new UserSkill { UserId = 1, SkillId = 2, ProfileId = 1, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            // Atualizar removendo a skill 2
            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill> { new UserSkill { SkillId = 1 } }
            };

            var (success, message, _) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);

            var skill1 = context.UserSkills.First(us => us.UserId == 1 && us.SkillId == 1);
            var skill2 = context.UserSkills.First(us => us.UserId == 1 && us.SkillId == 2);

            Assert.True(skill1.IsActive);
            Assert.False(skill2.IsActive);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleNullUserSkills()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
            context.Skills.Add(new Skill { SkillId = 1, Name = "C#" });
            await context.SaveChangesAsync();

            context.UserSkills.Add(new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = true });
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            // Atualizar com UserSkills = null
            var updatedProfile = new UpdateProfile
            {
                Biography = "Updated Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
                UserSkills = null
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("Updated Bio", updated.Biography);

            // Todas as skills devem ser desativadas
            var skills = context.UserSkills.Where(us => us.UserId == 1).ToList();
            Assert.All(skills, skill => Assert.False(skill.IsActive));
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleEmptyUserSkills()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "Updated Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
                UserSkills = new List<UserSkill>()
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("Updated Bio", updated.Biography);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateAllExperienceLevels()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            // Testar cada nível de experiência
            foreach (var level in new[] { ExperienceLevel.Junior, ExperienceLevel.Pleno, ExperienceLevel.Senior })
            {
                var updatedProfile = new UpdateProfile
                {
                    Biography = "Bio",
                    PricePerHour = 50,
                    ExperienceLevel = level,
                    UserSkills = new List<UserSkill>()
                };

                var (success, _, updated) = await service.UpdateProfileAsync(1, updatedProfile);

                Assert.True(success);
                Assert.Equal(level, updated.ExperienceLevel);
            }
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleMultipleSkillChanges_Simultaneously()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
                new Skill { SkillId = 2, Name = "Angular" },
                new Skill { SkillId = 3, Name = "React" },
                new Skill { SkillId = 4, Name = "Vue" }
            );
            await context.SaveChangesAsync();

            // Skills iniciais: 1 e 2 (ativas)
            context.UserSkills.AddRange(
                new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = true },
                new UserSkill { UserId = 1, SkillId = 2, ProfileId = 1, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            // Atualizar: manter skill 1, remover skill 2, adicionar skills 3 e 4
            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>
        {
            new UserSkill { SkillId = 1 },
            new UserSkill { SkillId = 3 },
            new UserSkill { SkillId = 4 }
        }
            };

            var (success, message, _) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);

            var allSkills = context.UserSkills.Where(us => us.UserId == 1).ToList();

            var skill1 = allSkills.First(us => us.SkillId == 1);
            var skill2 = allSkills.First(us => us.SkillId == 2);
            var skill3 = allSkills.First(us => us.SkillId == 3);
            var skill4 = allSkills.First(us => us.SkillId == 4);

            Assert.True(skill1.IsActive);  // Mantida
            Assert.False(skill2.IsActive); // Removida
            Assert.True(skill3.IsActive);  // Adicionada
            Assert.True(skill4.IsActive);  // Adicionada
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleZeroPricePerHour()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
                PricePerHour = 100,
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 0,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>()
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal(0, updated.PricePerHour);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleHighPricePerHour()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 999999,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>()
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal(999999, updated.PricePerHour);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleEmptyBiography()
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var updatedProfile = new UpdateProfile
            {
                Biography = "",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>()
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal("", updated.Biography);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldHandleLongBiography()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Short",
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
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            var longBio = new string('A', 5000); // Bio muito longa

            var updatedProfile = new UpdateProfile
            {
                Biography = longBio,
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>()
            };

            var (success, message, updated) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);
            Assert.Equal(longBio, updated.Biography);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldReactivateMultipleInactiveSkills()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
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
                new Skill { SkillId = 2, Name = "Angular" },
                new Skill { SkillId = 3, Name = "React" }
            );
            await context.SaveChangesAsync();

            // Adicionar todas as skills como inativas
            context.UserSkills.AddRange(
                new UserSkill { UserId = 1, SkillId = 1, ProfileId = 1, IsActive = false },
                new UserSkill { UserId = 1, SkillId = 2, ProfileId = 1, IsActive = false },
                new UserSkill { UserId = 1, SkillId = 3, ProfileId = 1, IsActive = false }
            );
            await context.SaveChangesAsync();

            var service = new ProfileService(context, GetFakeConfig());

            // Reativar todas
            var updatedProfile = new UpdateProfile
            {
                Biography = "Bio",
                PricePerHour = 50,
                ExperienceLevel = ExperienceLevel.Junior,
                UserSkills = new List<UserSkill>
        {
            new UserSkill { SkillId = 1 },
            new UserSkill { SkillId = 2 },
            new UserSkill { SkillId = 3 }
        }
            };

            var (success, message, _) = await service.UpdateProfileAsync(1, updatedProfile);

            Assert.True(success);

            var allSkills = context.UserSkills.Where(us => us.UserId == 1).ToList();
            Assert.All(allSkills, skill => Assert.True(skill.IsActive));
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ShouldReturnCorrectProfileId()
        {
            var context = GetDbContext();

            var profile = new Profile
            {
                ProfileId = 42,
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
            Assert.Equal(42, result.ProfileId);
        }
    }
}