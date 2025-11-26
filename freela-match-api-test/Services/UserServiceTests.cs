using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Configuration;

namespace freela_match_api_test.Services
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

        // ---------------------------------------------------------
        // GET USER BY ID - CENÁRIOS ADICIONAIS
        // ---------------------------------------------------------

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsUser_WithoutProfile()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 1,
                Name = "John",
                Email = "john@test.com",
                Password = "pass123",
                Token = "token123",
                Profile = null // Sem perfil
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Null(result.Profile);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsUser_WithProfileButNoSkills()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 2,
                Name = "Jane",
                Email = "jane@test.com",
                Password = "pass123",
                Token = "token123",
                Profile = new Profile
                {
                    ProfileId = 1,
                    UserId = 2,
                    Biography = "Designer",
                    ExperienceLevel = ExperienceLevel.Senior,
                    PricePerHour = 200,
                    UserSkills = new List<UserSkill>() // Lista vazia
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(2);

            Assert.NotNull(result);
            Assert.NotNull(result.Profile);
            Assert.Empty(result.Profile.UserSkills);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsUser_WithInactiveSkills()
        {
            var context = GetDbContext();

            var skill1 = new Skill { SkillId = 1, Name = "Python" };
            context.Skills.Add(skill1);
            await context.SaveChangesAsync();

            var user = new User
            {
                Id = 3,
                Name = "Bob",
                Email = "bob@test.com",
                Password = "pass123",
                Token = "token123",
                Profile = new Profile
                {
                    ProfileId = 2,
                    UserId = 3,
                    Biography = "Dev",
                    ExperienceLevel = ExperienceLevel.Pleno,
                    PricePerHour = 150
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Adicionar skill inativa
            context.UserSkills.Add(new UserSkill
            {
                UserId = 3,
                ProfileId = 2,
                SkillId = 1,
                IsActive = false
            });
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(3);

            Assert.NotNull(result);
            Assert.NotNull(result.Profile);
            // UserSkills deve incluir a skill inativa também
            Assert.Single(result.UserSkills);
            Assert.False(result.UserSkills.First().IsActive);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsUser_WithMultipleActiveAndInactiveSkills()
        {
            var context = GetDbContext();

            var skill1 = new Skill { SkillId = 1, Name = "Java" };
            var skill2 = new Skill { SkillId = 2, Name = "Spring" };
            var skill3 = new Skill { SkillId = 3, Name = "Docker" };

            context.Skills.AddRange(skill1, skill2, skill3);
            await context.SaveChangesAsync();

            var user = new User
            {
                Id = 4,
                Name = "Alice",
                Email = "alice@test.com",
                Password = "pass123",
                Token = "token123",
                Profile = new Profile
                {
                    ProfileId = 3,
                    UserId = 4,
                    Biography = "Backend Dev",
                    ExperienceLevel = ExperienceLevel.Senior,
                    PricePerHour = 180
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.UserSkills.AddRange(
                new UserSkill { UserId = 4, ProfileId = 3, SkillId = 1, IsActive = true },
                new UserSkill { UserId = 4, ProfileId = 3, SkillId = 2, IsActive = false },
                new UserSkill { UserId = 4, ProfileId = 3, SkillId = 3, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(4);

            Assert.NotNull(result);
            Assert.Equal(3, result.UserSkills.Count);
            Assert.Equal(2, result.UserSkills.Count(us => us.IsActive));
            Assert.Single(result.UserSkills.Where(us => !us.IsActive));
        }

        [Fact]
        public async Task GetUserByUserIdAsync_LoadsSkillNavigationProperty()
        {
            var context = GetDbContext();

            var skill = new Skill { SkillId = 1, Name = "TypeScript" };
            context.Skills.Add(skill);
            await context.SaveChangesAsync();

            var user = new User
            {
                Id = 5,
                Name = "Charlie",
                Email = "charlie@test.com",
                Password = "pass123",
                Token = "token123",
                Profile = new Profile
                {
                    ProfileId = 4,
                    UserId = 5,
                    Biography = "Frontend",
                    ExperienceLevel = ExperienceLevel.Pleno,
                    PricePerHour = 140
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.UserSkills.Add(new UserSkill
            {
                UserId = 5,
                ProfileId = 4,
                SkillId = 1,
                IsActive = true
            });
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(5);

            Assert.NotNull(result);
            Assert.Single(result.UserSkills);
            Assert.NotNull(result.UserSkills.First().Skill);
            Assert.Equal("TypeScript", result.UserSkills.First().Skill.Name);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(12345)]
        public async Task GetUserByUserIdAsync_WorksWithDifferentUserIds(int userId)
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = userId,
                Name = $"User {userId}",
                Email = $"user{userId}@test.com",
                Password = "pass123",
                Token = "token123"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsNull_ForZeroUserId()
        {
            var context = GetDbContext();
            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(0);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsNull_ForNegativeUserId()
        {
            var context = GetDbContext();
            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(-1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUserIdAsync_ReturnsCorrectUser_WhenMultipleUsersExist()
        {
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "User 1", Email = "u1@test.com", Password = "pass", Token = "t1" },
                new User { Id = 2, Name = "User 2", Email = "u2@test.com", Password = "pass", Token = "t2" },
                new User { Id = 3, Name = "User 3", Email = "u3@test.com", Password = "pass", Token = "t3" }
            );
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var result = await service.GetUserByUserIdAsync(2);

            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("User 2", result.Name);
        }

        // ---------------------------------------------------------
        // UPDATE USER - CENÁRIOS ADICIONAIS
        // ---------------------------------------------------------

        [Fact]
        public async Task UpdateUserAsync_UpdatesOnlyName()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 10,
                Name = "Old Name",
                Email = "test@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "New Name",
                IsAvailable = true // Mantém o mesmo valor
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(10, dto);

            Assert.True(success);
            Assert.Equal("New Name", updatedUser.Name);
            Assert.True(updatedUser.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesOnlyIsAvailable()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 11,
                Name = "John Doe",
                Email = "john@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "John Doe", // Mantém o mesmo valor
                IsAvailable = true
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(11, dto);

            Assert.True(success);
            Assert.Equal("John Doe", updatedUser.Name);
            Assert.True(updatedUser.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesBothFields()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 12,
                Name = "Old Name",
                Email = "old@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Completely New Name",
                IsAvailable = true
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(12, dto);

            Assert.True(success);
            Assert.Equal("Completely New Name", updatedUser.Name);
            Assert.True(updatedUser.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_SetsIsAvailableToFalse()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 13,
                Name = "Available User",
                Email = "available@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Available User",
                IsAvailable = false
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(13, dto);

            Assert.True(success);
            Assert.False(updatedUser.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_PersistsChangesToDatabase()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 14,
                Name = "Original",
                Email = "original@test.com",
                Password = "pass123",
                Token = "token123",
                IsAvailable = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Updated",
                IsAvailable = true
            };

            await service.UpdateUserAsync(14, dto);

            // Buscar novamente do banco para verificar persistência
            var userFromDb = await context.Users.FindAsync(14);

            Assert.NotNull(userFromDb);
            Assert.Equal("Updated", userFromDb.Name);
            Assert.True(userFromDb.IsAvailable);
        }

        [Fact]
        public async Task UpdateUserAsync_DoesNotModifyOtherFields()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 15,
                Name = "Original Name",
                Email = "original@test.com",
                Password = "secret123",
                Token = "token123",
                IsAvailable = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "New Name",
                IsAvailable = true
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(15, dto);

            Assert.True(success);
            // Verificar que outros campos não foram modificados
            Assert.Equal("original@test.com", updatedUser.Email);
            Assert.Equal("secret123", updatedUser.Password);
            Assert.Equal("token123", updatedUser.Token);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task UpdateUserAsync_WorksWithDifferentUserIds(int userId)
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = userId,
                Name = "Original",
                Email = $"user{userId}@test.com",
                Password = "pass",
                Token = "token",
                IsAvailable = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Updated",
                IsAvailable = true
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(userId, dto);

            Assert.True(success);
            Assert.Equal(userId, updatedUser.Id);
            Assert.Equal("Updated", updatedUser.Name);
        }

        [Fact]
        public async Task UpdateUserAsync_HandlesEmptyName()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 16,
                Name = "Old Name",
                Email = "test@test.com",
                Password = "pass",
                Token = "token",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "",
                IsAvailable = false
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(16, dto);

            Assert.True(success);
            Assert.Equal("", updatedUser.Name);
        }

        [Fact]
        public async Task UpdateUserAsync_HandlesLongName()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 17,
                Name = "Short",
                Email = "test@test.com",
                Password = "pass",
                Token = "token",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var longName = new string('A', 500);

            var dto = new UpdateUser
            {
                Name = longName,
                IsAvailable = false
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(17, dto);

            Assert.True(success);
            Assert.Equal(longName, updatedUser.Name);
        }

        [Fact]
        public async Task UpdateUserAsync_HandlesSpecialCharactersInName()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 18,
                Name = "Normal Name",
                Email = "test@test.com",
                Password = "pass",
                Token = "token",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var specialName = "João José O'Brien-Smith";

            var dto = new UpdateUser
            {
                Name = specialName,
                IsAvailable = false
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(18, dto);

            Assert.True(success);
            Assert.Equal(specialName, updatedUser.Name);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsCorrectMessage()
        {
            var context = GetDbContext();

            var user = new User
            {
                Id = 19,
                Name = "Test",
                Email = "test@test.com",
                Password = "pass",
                Token = "token",
                IsAvailable = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Updated",
                IsAvailable = false
            };

            var (success, message, updatedUser) = await service.UpdateUserAsync(19, dto);

            Assert.True(success);
            Assert.Equal("User updated successfully", message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task UpdateUserAsync_ReturnsError_ForInvalidUserId(int invalidUserId)
        {
            var context = GetDbContext();
            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Test",
                IsAvailable = true
            };

            var (success, message, user) = await service.UpdateUserAsync(invalidUserId, dto);

            Assert.False(success);
            Assert.Equal("User not found", message);
            Assert.Null(user);
        }

        [Fact]
        public async Task UpdateUserAsync_DoesNotAffectOtherUsers()
        {
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "User 1", Email = "u1@test.com", Password = "p1", Token = "t1", IsAvailable = true },
                new User { Id = 2, Name = "User 2", Email = "u2@test.com", Password = "p2", Token = "t2", IsAvailable = false },
                new User { Id = 3, Name = "User 3", Email = "u3@test.com", Password = "p3", Token = "t3", IsAvailable = true }
            );
            await context.SaveChangesAsync();

            var service = new UserService(context, GetConfigMock());

            var dto = new UpdateUser
            {
                Name = "Updated User 2",
                IsAvailable = true
            };

            await service.UpdateUserAsync(2, dto);

            var user1 = await context.Users.FindAsync(1);
            var user2 = await context.Users.FindAsync(2);
            var user3 = await context.Users.FindAsync(3);

            Assert.Equal("User 1", user1.Name); // Não modificado
            Assert.Equal("Updated User 2", user2.Name); // Modificado
            Assert.Equal("User 3", user3.Name); // Não modificado
        }
    }
}
