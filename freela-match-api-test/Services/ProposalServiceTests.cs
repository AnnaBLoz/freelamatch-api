using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace freela_match_api_test.Services
{
    public class ProposalServiceFullTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private Proposal CreateTestProposal(int proposalId = 1, int ownerId = 1, bool isAvailable = true)
        {
            return new Proposal
            {
                ProposalId = proposalId,
                OwnerId = ownerId,
                Title = "Test Title",
                Description = "Test Description",
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(5),
                IsAvailable = isAvailable,
                CreatedDate = DateTime.UtcNow
            };
        }

        // -----------------------------
        // CREATE PROPOSAL
        // -----------------------------
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

        // -----------------------------
        // APPROVE CANDIDATE
        // -----------------------------
        [Fact]
        public async Task ApproveCandidate_UpdatesStatuses_AndSendsEmail()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendApproveEmail(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 1, ownerId: 10));

            context.Candidate.AddRange(
                new Candidate { CandidateId = 1, ProposalId = 1, UserId = 100, Message = "msg", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending },
                new Candidate { CandidateId = 2, ProposalId = 1, UserId = 200, Message = "msg2", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending }
            );
            await context.SaveChangesAsync();

            var result = await service.ApproveCandidate(new CandidateApprove { CandidateId = 1, ProposalId = 1 });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Accepted, context.Candidate.First(c => c.CandidateId == 1).Status);
            Assert.Equal(ProposalStatus.Rejected, context.Candidate.First(c => c.CandidateId == 2).Status);
            emailMock.Verify(e => e.SendApproveEmail(1, 100), Times.Once);
        }

        // -----------------------------
        // APPROVE CANDIDATE - NOT FOUND
        // -----------------------------
        [Fact]
        public async Task ApproveCandidate_CandidateNotFound_ReturnsFalse()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.ApproveCandidate(new CandidateApprove { CandidateId = 999, ProposalId = 1 });

            Assert.False(result.Success);
            Assert.Equal("Candidate not found", result.Message);
            Assert.Null(result.Candidate);
        }

        // -----------------------------
        // DISAPPROVE CANDIDATE
        // -----------------------------
        [Fact]
        public async Task DisapproveCandidate_SetsRejected()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal());
            context.Candidate.Add(new Candidate { CandidateId = 10, ProposalId = 1, UserId = 100, Message = "msg", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending });
            await context.SaveChangesAsync();

            var result = await service.DisapproveCandidate(new CandidateApprove { CandidateId = 10, ProposalId = 1 });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Rejected, context.Candidate.First().Status);
        }

        // -----------------------------
        // DISAPPROVE CANDIDATE - NOT FOUND
        // -----------------------------
        [Fact]
        public async Task DisapproveCandidate_CandidateNotFound_ReturnsFalse()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.DisapproveCandidate(new CandidateApprove { CandidateId = 999, ProposalId = 1 });

            Assert.False(result.Success);
            Assert.Equal("Candidate not found", result.Message);
            Assert.Null(result.Candidate);
        }

        // -----------------------------
        // NEW CANDIDATE
        // -----------------------------
        [Fact]
        public async Task Candidate_CreatesCandidate_AndSendsEmail()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendNewCandidateEmailAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            var service = new ProposalService(context, emailMock.Object);

            var dto = new CandidateProposal { ProposalId = 5, UserId = 33, EstimatedDate = DateTime.UtcNow.AddDays(2).ToString(), ProposedPrice = 150, Message = "Test" };
            var result = await service.Candidate(dto);

            Assert.NotNull(result);
            Assert.Equal(33, result.UserId);
            emailMock.Verify(e => e.SendNewCandidateEmailAsync(5, 33), Times.Once);
        }

        // -----------------------------
        // COUNTER PROPOSAL
        // -----------------------------
        [Fact]
        public async Task CounterProposal_CreatesAndSendsEmail()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendCounterProposalEmailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 7, ownerId: 99));
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
            emailMock.Verify(e => e.SendCounterProposalEmailAsync(7, 12, It.IsAny<int>()), Times.Once);
        }

        // -----------------------------
        // COUNTER PROPOSAL - PROPOSAL NOT FOUND
        // -----------------------------
        [Fact]
        public async Task CounterProposal_ProposalNotFound_ReturnsFalse()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var dto = new CounterProposalCreate { ProposalId = 999, FreelancerId = 1, CompanyId = 1, EstimatedDate = DateTime.UtcNow, ProposedPrice = 100, Message = "Test", IsAccepted = false, IsSendedByCompany = true };
            var result = await service.CounterProposal(dto);

            Assert.False(result.Success);
            Assert.Equal("Proposal not found", result.Message);
            Assert.Null(result.Proposal);
        }

        // -----------------------------
        // GET PROPOSALS
        // -----------------------------
        [Fact]
        public async Task GetProposals_ReturnsProposalsForCompany()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 1, ownerId: 10));
            context.Proposal.Add(CreateTestProposal(proposalId: 2, ownerId: 20));
            await context.SaveChangesAsync();

            var result = await service.GetProposals(10);

            Assert.Single(result);
            Assert.Equal(10, result.First().OwnerId);
        }

        // -----------------------------
        // GET ALL PROPOSALS
        // -----------------------------
        [Fact]
        public async Task GetAllProposals_ReturnsAvailableProposals()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 1, isAvailable: true));
            context.Proposal.Add(CreateTestProposal(proposalId: 2, isAvailable: false));
            await context.SaveChangesAsync();

            var result = await service.GetAllProposals();

            Assert.Single(result);
            Assert.True(result.First().IsAvailable);
        }

        // -----------------------------
        // GET PROPOSAL BY ID
        // -----------------------------
        [Fact]
        public async Task GetProposalById_ReturnsProposal()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 1));
            await context.SaveChangesAsync();

            var result = await service.GetProposalById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProposalId);
        }

        // -----------------------------
        // GET PROPOSAL BY ID AND CANDIDATE
        // -----------------------------
        [Fact]
        public async Task GetProposalByIdAndCandidate_ReturnsProposalWithCandidate()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var proposal = CreateTestProposal(proposalId: 1);
            proposal.Candidates = new List<Candidate>
            {
                new Candidate
                {
                    CandidateId = 100,
                    UserId = 5,
                    ProposalId = 1,
                    EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(),
                    Message = "Test message",
                    Status = ProposalStatus.Pending
                }
            };
            context.Proposal.Add(proposal);
            await context.SaveChangesAsync();

            var result = await service.GetProposalByIdAndCandidate(1, 5);

            Assert.NotNull(result);
            Assert.Single(result.Candidates);
            Assert.Equal(5, result.Candidates.First().UserId);
        }

        // -----------------------------
        // GET COUNTER PROPOSALS
        // -----------------------------
        [Fact]
        public async Task GetCounterProposalByProposalId_ReturnsCounters()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(CreateTestProposal(proposalId: 1));
            context.CounterProposal.Add(new CounterProposal { CounterProposalId = 1, ProposalId = 1 });
            await context.SaveChangesAsync();

            var result = await service.GetCounterProposalByProposalId(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProposalId);
        }

        // -----------------------------
        // GET PROPOSALS BY USER ID
        // -----------------------------
        [Fact]
        public async Task GetProposalsByUserId_ReturnsProposals()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var proposal = CreateTestProposal(proposalId: 1);
            proposal.Candidates = new List<Candidate>
            {
                new Candidate
                {
                    UserId = 5,
                    CandidateId = 1,
                    EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(),
                    Message = "Test message",
                    Status = ProposalStatus.Pending
                }
            };
            context.Proposal.Add(proposal);
            await context.SaveChangesAsync();

            var result = await service.GetProposalsByUserId(5);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProposalId);
        }

        // -----------------------------
        // GET FREELANCERS TO REVIEW - EMPTY
        // -----------------------------
        [Fact]
        public async Task GetFreelancersToReview_ReturnsEmptyListWhenNone()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetFreelancersToReview(1);
            Assert.Empty(result);
        }

        // -----------------------------
        // GET COMPANIES TO REVIEW - EMPTY
        // -----------------------------
        [Fact]
        public async Task GetCompaniesToReview_ReturnsEmptyListWhenNone()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetCompaniesToReview(1);
            Assert.Empty(result);
        }
    }
}
