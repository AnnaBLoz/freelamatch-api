using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace freela_match_api_test
{
    public class ProposalServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // ---------------------------------------------------------
        // CREATE PROPOSAL
        // ---------------------------------------------------------
        [Fact]
        public async Task CreateProposal_CreatesProposalWithSkills()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();

            var service = new ProposalService(context, emailMock.Object);

            var dto = new CreateProposal
            {
                Title = "Test",
                Description = "Desc",
                Price = 200,
                MaxDate = DateTime.UtcNow.AddDays(5),
                OwnerId = 10,
                RequiredSkills = new List<ProposalSkillCreate>
                {
                    new ProposalSkillCreate { SkillId = 1 },
                    new ProposalSkillCreate { SkillId = 2 }
                }
            };

            var result = await service.CreateProposal(dto);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Title);
            Assert.Equal(2, context.ProposalSkill.Count());
        }

        // ---------------------------------------------------------
        // APPROVE CANDIDATE
        // ---------------------------------------------------------
        [Fact]
        public async Task ApproveCandidate_UpdatesStatuses_AndSendsEmail()
        {
            var context = GetDbContext();

            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendApproveEmail(It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal { ProposalId = 1, OwnerId = 10, IsAvailable = true });
            context.Candidate.AddRange(
                new Candidate { CandidateId = 1, ProposalId = 1, UserId = 100 },
                new Candidate { CandidateId = 2, ProposalId = 1, UserId = 200 }
            );
            await context.SaveChangesAsync();

            var result = await service.ApproveCandidate(new CandidateApprove
            {
                CandidateId = 1,
                ProposalId = 1
            });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Accepted, context.Candidate.First(c => c.CandidateId == 1).Status);
            Assert.Equal(ProposalStatus.Rejected, context.Candidate.First(c => c.CandidateId == 2).Status);

            emailMock.Verify(e => e.SendApproveEmail(1, 1), Times.Once);
        }

        // ---------------------------------------------------------
        // DISAPPROVE CANDIDATE
        // ---------------------------------------------------------
        [Fact]
        public async Task DisapproveCandidate_SetsRejected()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();

            var service = new ProposalService(context, emailMock.Object);

            context.Candidate.Add(new Candidate
            {
                CandidateId = 10,
                ProposalId = 1,
                UserId = 100,
                Status = ProposalStatus.Pending
            });
            await context.SaveChangesAsync();

            var result = await service.DisapproveCandidate(new CandidateApprove
            {
                CandidateId = 10,
                ProposalId = 1
            });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Rejected, context.Candidate.First().Status);
        }

        // ---------------------------------------------------------
        // NEW CANDIDATE
        // ---------------------------------------------------------
        [Fact]
        public async Task Candidate_CreatesCandidate_AndSendsEmail()
        {
            var context = GetDbContext();

            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendNewCandidateEmailAsync(It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            var dto = new CandidateProposal
            {
                ProposalId = 5,
                UserId = 33,
                EstimatedDate = DateTime.UtcNow.ToString(),
                ProposedPrice = 150,
                Message = "Test"
            };

            var result = await service.Candidate(dto);

            Assert.NotNull(result);
            Assert.Equal(33, result.UserId);

            emailMock.Verify(e => e.SendNewCandidateEmailAsync(5, 33), Times.Once);
        }

        // ---------------------------------------------------------
        // COUNTER PROPOSAL
        // ---------------------------------------------------------
        [Fact]
        public async Task CounterProposal_CreatesAndSendsEmail()
        {
            var context = GetDbContext();

            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e =>
                e.SendCounterProposalEmailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
            ).Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal { ProposalId = 7, OwnerId = 99 });
            await context.SaveChangesAsync();

            var dto = new CounterProposalCreate
            {
                ProposalId = 7,
                FreelancerId = 12,
                CompanyId = 99,
                EstimatedDate = DateTime.UtcNow,
                ProposedPrice = 900,
                Message = "Counter",
                IsAccepted = false,
                IsSendedByCompany = true
            };

            var result = await service.CounterProposal(dto);

            Assert.True(result.Success);
            Assert.Single(context.CounterProposal.ToList());

            emailMock.Verify(e =>
                e.SendCounterProposalEmailAsync(7, 12, It.IsAny<int>()),
                Times.Once
            );
        }
    }
}
