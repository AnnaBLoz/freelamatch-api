using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace freela_match_api_test
{
    public class GeneralServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetFreelancers_ReturnsOnlyFreelancers()
        {
            // Arrange
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "Ana", Type = UserType.Freelancer },
                new User { Id = 2, Name = "Carlos", Type = UserType.Company },
                new User { Id = 3, Name = "Beatriz", Type = UserType.Freelancer }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);

            // Act
            var result = await service.GetFreelancers();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(UserType.Freelancer, r!.Type));
        }

        [Fact]
        public async Task GetSectors_ReturnsAllSectors()
        {
            // Arrange
            var context = GetDbContext();

            context.Sector.AddRange(
                new Sector { SectorId = 1, Name = "TI" },
                new Sector { SectorId = 2, Name = "Design" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);

            // Act
            var result = await service.GetSectors();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetSkills_ReturnsAllSkills()
        {
            // Arrange
            var context = GetDbContext();

            context.Skills.AddRange(
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);

            // Act
            var result = await service.GetSkills();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CompletedProjects_ReturnsOnlyAcceptedProposals()
        {
            // Arrange
            var context = GetDbContext();

            context.Candidate.AddRange(
                new Candidate { CandidateId = 1, UserId = 10, Status = ProposalStatus.Accepted },
                new Candidate { CandidateId = 2, UserId = 10, Status = ProposalStatus.Pending },
                new Candidate { CandidateId = 3, UserId = 10, Status = ProposalStatus.Accepted },
                new Candidate { CandidateId = 4, UserId = 5, Status = ProposalStatus.Accepted } // outro usuário
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);

            // Act
            var result = await service.CompletedProjects(10);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(ProposalStatus.Accepted, r!.Status));
            Assert.All(result, r => Assert.Equal(10, r!.UserId));
        }
    }
}
