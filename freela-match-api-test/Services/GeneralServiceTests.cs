using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region GetFreelancers Tests

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
        public async Task GetFreelancers_ReturnsEmptyList_WhenNoFreelancers()
        {
            var context = GetDbContext();

            context.Users.Add(
                new User { Id = 1, Name = "Company", Type = UserType.Company, Email = "c@mail.com", Password = "123", Token = "t1" }
            );
            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.GetFreelancers();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreelancers_IncludesProfileAndSkills()
        {
            var context = GetDbContext();

            var user = new User { Id = 1, Name = "Ana", Type = UserType.Freelancer, Email = "a@mail.com", Password = "123", Token = "t1" };
            var profile = new Profile { ProfileId = 1, UserId = 1, Biography = "Bio", ExperienceLevel = ExperienceLevel.Senior, PricePerHour = 50 };
            var skill = new Skill { SkillId = 1, Name = "C#" };
            var userSkill = new UserSkill { UserSkillId = 1, UserId = 1, SkillId = 1 };

            context.Users.Add(user);
            context.Profiles.Add(profile);
            context.Skills.Add(skill);
            context.UserSkills.Add(userSkill);
            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.GetFreelancers();

            Assert.Single(result);
            Assert.NotNull(result[0]!.Profile);
            Assert.NotNull(result[0]!.Profile!.UserSkills);
            Assert.Single(result[0]!.Profile!.UserSkills);
        }

        #endregion

        #region GetSectors Tests

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
        public async Task GetSectors_ReturnsEmptyList_WhenNoSectors()
        {
            var context = GetDbContext();
            var service = new GeneralService(context);
            var result = await service.GetSectors();

            Assert.Empty(result);
        }

        #endregion

        #region GetSkills Tests

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
        public async Task GetSkills_ReturnsEmptyList_WhenNoSkills()
        {
            var context = GetDbContext();
            var service = new GeneralService(context);
            var result = await service.GetSkills();

            Assert.Empty(result);
        }

        #endregion

        #region CompletedProjects Tests

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
                new Candidate { CandidateId = 4, UserId = 5, ProposalId = 1, Status = ProposalStatus.Accepted, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() }
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.CompletedProjects(10);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(ProposalStatus.Accepted, r!.Status));
            Assert.All(result, r => Assert.Equal(10, r!.UserId));
        }

        [Fact]
        public async Task CompletedProjects_ReturnsEmptyList_WhenNoAcceptedProposals()
        {
            var context = GetDbContext();

            context.Proposal.Add(
                new Proposal { ProposalId = 1, Title = "P1", Description = "D1", Price = 100, MaxDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow, OwnerId = 10, IsAvailable = false }
            );

            context.Candidate.Add(
                new Candidate { CandidateId = 1, UserId = 10, ProposalId = 1, Status = ProposalStatus.Pending, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() }
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.CompletedProjects(10);

            Assert.Empty(result);
        }

        [Fact]
        public async Task CompletedProjects_ReturnsEmptyList_WhenUserIdNotFound()
        {
            var context = GetDbContext();

            context.Candidate.Add(
                new Candidate { CandidateId = 1, UserId = 10, ProposalId = 1, Status = ProposalStatus.Accepted, Message = "msg", EstimatedDate = DateTime.UtcNow.ToString() }
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.CompletedProjects(999);

            Assert.Empty(result);
        }

        #endregion

        #region Match Tests

        [Fact]
        public async Task Match_ReturnsTopFreelancers_OrderedBySkillsAndRating()
        {
            var context = GetDbContext();

            // Skills
            var skills = new List<Skill>
            {
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "Angular" },
                new Skill { SkillId = 3, Name = "React" }
            };
            context.Skills.AddRange(skills);

            // Company user
            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            // Proposal with required skills
            var proposal = new Proposal
            {
                ProposalId = 1,
                Title = "Project",
                Description = "Desc",
                Price = 1000,
                MaxDate = DateTime.UtcNow.AddDays(30),
                CreatedDate = DateTime.UtcNow,
                OwnerId = 100,
                IsAvailable = true
            };
            context.Proposal.Add(proposal);

            var requiredSkills = new List<ProposalSkill>
            {
                new ProposalSkill { ProposalSkillId = 1, ProposalId = 1, SkillId = 1 },
                new ProposalSkill { ProposalSkillId = 2, ProposalId = 1, SkillId = 2 }
            };
            context.ProposalSkill.AddRange(requiredSkills);

            // Freelancers
            var freelancer1 = new User { Id = 1, Name = "Dev1", Type = UserType.Freelancer, Email = "dev1@mail.com", Password = "123", Token = "t1", IsAvailable = true };
            var freelancer2 = new User { Id = 2, Name = "Dev2", Type = UserType.Freelancer, Email = "dev2@mail.com", Password = "123", Token = "t2", IsAvailable = true };
            var freelancer3 = new User { Id = 3, Name = "Dev3", Type = UserType.Freelancer, Email = "dev3@mail.com", Password = "123", Token = "t3", IsAvailable = true };

            context.Users.AddRange(freelancer1, freelancer2, freelancer3);

            // UserSkills - freelancer1 tem 2 matches, freelancer2 tem 1, freelancer3 tem 2
            context.UserSkills.AddRange(
                new UserSkill { UserSkillId = 1, UserId = 1, SkillId = 1 },
                new UserSkill { UserSkillId = 2, UserId = 1, SkillId = 2 },
                new UserSkill { UserSkillId = 3, UserId = 2, SkillId = 1 },
                new UserSkill { UserSkillId = 4, UserId = 3, SkillId = 1 },
                new UserSkill { UserSkillId = 5, UserId = 3, SkillId = 2 }
            );

            // Reviews - freelancer3 tem melhor avaliação
            context.Reviews.AddRange(
                new Reviews { Id = 1, ReviewerId = 1, ReceiverId = 100, Rating = 4, ReviewText = "Good", CreatedAt = DateTime.UtcNow },
                new Reviews { Id = 2, ReviewerId = 3, ReceiverId = 100, Rating = 5, ReviewText = "Excellent", CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Equal(3, result.Count);
            // Ambos têm 2 skills, mas freelancer3 tem melhor rating
            Assert.Equal(3, result[0].Id); // Dev3 (2 skills, rating 5)
            Assert.Equal(1, result[1].Id); // Dev1 (2 skills, rating 4)
            Assert.Equal(2, result[2].Id); // Dev2 (1 skill)
        }

        [Fact]
        public async Task Match_ReturnsEmptyList_WhenNoOpenProposals()
        {
            var context = GetDbContext();

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            // Proposal fechada
            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = false,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Match_ReturnsEmptyList_WhenNoRequiredSkills()
        {
            var context = GetDbContext();

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            // Proposal sem skills
            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = true,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Match_ReturnsOnlyAvailableFreelancers()
        {
            var context = GetDbContext();

            var skill = new Skill { SkillId = 1, Name = "C#" };
            context.Skills.Add(skill);

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = true,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);
            context.ProposalSkill.Add(new ProposalSkill { ProposalSkillId = 1, ProposalId = 1, SkillId = 1 });

            // Um freelancer disponível, um não
            var freelancer1 = new User { Id = 1, Name = "Dev1", Type = UserType.Freelancer, Email = "dev1@mail.com", Password = "123", Token = "t1", IsAvailable = true };
            var freelancer2 = new User { Id = 2, Name = "Dev2", Type = UserType.Freelancer, Email = "dev2@mail.com", Password = "123", Token = "t2", IsAvailable = false };
            context.Users.AddRange(freelancer1, freelancer2);

            context.UserSkills.AddRange(
                new UserSkill { UserSkillId = 1, UserId = 1, SkillId = 1 },
                new UserSkill { UserSkillId = 2, UserId = 2, SkillId = 1 }
            );

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task Match_ReturnsMaximumFiveFreelancers()
        {
            var context = GetDbContext();

            var skill = new Skill { SkillId = 1, Name = "C#" };
            context.Skills.Add(skill);

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = true,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);
            context.ProposalSkill.Add(new ProposalSkill { ProposalSkillId = 1, ProposalId = 1, SkillId = 1 });

            // 7 freelancers com a skill
            for (int i = 1; i <= 7; i++)
            {
                var freelancer = new User
                {
                    Id = i,
                    Name = $"Dev{i}",
                    Type = UserType.Freelancer,
                    Email = $"dev{i}@mail.com",
                    Password = "123",
                    Token = $"t{i}",
                    IsAvailable = true
                };
                context.Users.Add(freelancer);
                context.UserSkills.Add(new UserSkill { UserSkillId = i, UserId = i, SkillId = 1 });
            }

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Match_MapsUserResumeCorrectly()
        {
            var context = GetDbContext();

            var skill = new Skill { SkillId = 1, Name = "C#" };
            context.Skills.Add(skill);

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = true,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);
            context.ProposalSkill.Add(new ProposalSkill { ProposalSkillId = 1, ProposalId = 1, SkillId = 1 });

            var freelancer = new User { Id = 1, Name = "Dev1", Type = UserType.Freelancer, Email = "dev1@mail.com", Password = "123", Token = "t1", IsAvailable = true };
            context.Users.Add(freelancer);

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                Biography = "Bio",
                ExperienceLevel = ExperienceLevel.Senior,
                PricePerHour = 50
            };
            context.Profiles.Add(profile);

            context.UserSkills.Add(new UserSkill { UserSkillId = 1, UserId = 1, SkillId = 1 });

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Single(result);
            var userResume = result[0];
            Assert.Equal(1, userResume.Id);
            Assert.Equal("Dev1", userResume.Name);
            Assert.Equal(UserType.Freelancer, userResume.Type);
            Assert.True(userResume.IsAvailable);
            Assert.NotNull(userResume.Profile);
            Assert.Equal("Bio", userResume.Profile.Biography);
            Assert.Equal(ExperienceLevel.Senior, userResume.Profile.ExperienceLevel);
            Assert.Equal(50, userResume.Profile.PricePerHour);
            Assert.Single(userResume.UserSkills);
            Assert.Equal(1, userResume.UserSkills.FirstOrDefault().SkillId);
            Assert.Equal("C#", userResume.UserSkills.FirstOrDefault().Name);
        }

        [Fact]
        public async Task Match_HandlesFreelancersWithoutProfile()
        {
            var context = GetDbContext();

            var skill = new Skill { SkillId = 1, Name = "C#" };
            context.Skills.Add(skill);

            var company = new User { Id = 100, Name = "Company", Type = UserType.Company, Email = "company@mail.com", Password = "123", Token = "tc" };
            context.Users.Add(company);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 100,
                IsAvailable = true,
                Title = "P",
                Description = "D",
                Price = 100,
                MaxDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Proposal.Add(proposal);
            context.ProposalSkill.Add(new ProposalSkill { ProposalSkillId = 1, ProposalId = 1, SkillId = 1 });

            var freelancer = new User { Id = 1, Name = "Dev1", Type = UserType.Freelancer, Email = "dev1@mail.com", Password = "123", Token = "t1", IsAvailable = true };
            context.Users.Add(freelancer);
            context.UserSkills.Add(new UserSkill { UserSkillId = 1, UserId = 1, SkillId = 1 });

            await context.SaveChangesAsync();

            var service = new GeneralService(context);
            var result = await service.Match(100);

            Assert.Single(result);
            Assert.Null(result[0].Profile);
        }

        #endregion
    }
}