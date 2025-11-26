using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace freela_match_api_test.Services
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
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "Ana", Type = UserType.Freelancer, Email = "a@mail.com", Password = "123", Token = "t1" },
                new User { Id = 2, Name = "Carlos", Type = UserType.Company, Email = "c@mail.com", Password = "123", Token = "t2" },
                new User { Id = 3, Name = "Beatriz", Type = UserType.Freelancer, Email = "b@mail.com", Password = "123", Token = "t3" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.GetFreelancers();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(UserType.Freelancer, r!.Type));
        }

        [Fact]
        public async Task GetSectors_ReturnsAllSectors()
        {
            var context = GetDbContext();

            context.Sector.AddRange(
                new Sector { SectorId = 1, Name = "TI" },
                new Sector { SectorId = 2, Name = "Design" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.GetSectors();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetSkills_ReturnsAllSkills()
        {
            var context = GetDbContext();

            context.Skills.AddRange(
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.GetSkills();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CompletedProjects_ReturnsOnlyAcceptedProposals()
        {
            var context = GetDbContext();

            context.Proposal.AddRange(
                new Proposal { ProposalId = 1, Title = "P1", Description = "D1", Price = 100, MaxDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow, OwnerId = 10, IsAvailable = false },
                new Proposal { ProposalId = 2, Title = "P2", Description = "D2", Price = 100, MaxDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow, OwnerId = 10, IsAvailable = false }
            );

            context.Candidate.AddRange(
                new Candidate { CandidateId = 1, UserId = 10, ProposalId = 1, Status = ProposalStatus.Accepted, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() },
                new Candidate { CandidateId = 2, UserId = 10, ProposalId = 2, Status = ProposalStatus.Pending, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() },
                new Candidate { CandidateId = 3, UserId = 10, ProposalId = 2, Status = ProposalStatus.Accepted, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() },
                new Candidate { CandidateId = 4, UserId = 5, ProposalId = 1, Status = ProposalStatus.Accepted, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() } // outro usuário
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.CompletedProjects(10);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(ProposalStatus.Accepted, r!.Status));
            Assert.All(result, r => Assert.Equal(10, r!.UserId));
        }
    }
}
