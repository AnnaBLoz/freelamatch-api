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
    public class ProposalServiceFullTests : IDisposable
    {
        private AppDbContext _context;
        private ProposalService _service;
        private Mock<IEmailService> _emailMock;

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        public ProposalServiceFullTests()
        {
            _context = GetDbContext();
            _emailMock = new Mock<IEmailService>();
            _service = new ProposalService(_context, _emailMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private Proposal CreateValidProposal(int proposalId = 1, int ownerId = 1, bool isAvailable = true)
        {
            return new Proposal
            {
                ProposalId = proposalId,
                OwnerId = ownerId,
                Title = $"Título {proposalId}",
                Description = $"Descrição {proposalId}",
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(5),
                CreatedDate = DateTime.UtcNow,
                IsAvailable = isAvailable,
                Candidates = new List<Candidate>()
            };
        }

        private CounterProposal CreateValidCounterProposal(int counterId = 1, int proposalId = 1, int freelancerId = 1)
        {
            return new CounterProposal
            {
                CounterProposalId = counterId,
                ProposalId = proposalId,
                FreelancerId = freelancerId,
                Message = $"Mensagem {counterId}",
                ProposedPrice = 200,
                EstimatedDate = DateTime.UtcNow,
                IsAccepted = false,
                IsSendedByCompany = true
            };
        }

        // -----------------------------
        // CREATE PROPOSAL
        // -----------------------------
        [Fact]
        public async Task CreateProposal_CreatesProposalWithSkills()
        {
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

            var result = await _service.CreateProposal(dto);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Title);
            Assert.Equal(2, _context.ProposalSkill.Count());
        }

        // -----------------------------
        // APPROVE CANDIDATE
        // -----------------------------
        [Fact]
        public async Task ApproveCandidate_UpdatesStatuses_AndSendsEmail()
        {
            var proposal = CreateValidProposal(proposalId: 1, ownerId: 10);
            _context.Proposal.Add(proposal);

            _context.Candidate.AddRange(
                new Candidate { CandidateId = 1, ProposalId = 1, UserId = 100, Message = "msg", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending },
                new Candidate { CandidateId = 2, ProposalId = 1, UserId = 200, Message = "msg2", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending }
            );

            await _context.SaveChangesAsync();

            _emailMock.Setup(e => e.SendApproveEmail(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _service.ApproveCandidate(new CandidateApprove { CandidateId = 1, ProposalId = 1 });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Accepted, _context.Candidate.First(c => c.CandidateId == 1).Status);
            Assert.Equal(ProposalStatus.Rejected, _context.Candidate.First(c => c.CandidateId == 2).Status);

            _emailMock.Verify(e => e.SendApproveEmail(1, 100), Times.Once);
        }

        [Fact]
        public async Task ApproveCandidate_CandidateNotFound_ReturnsFalse()
        {
            var result = await _service.ApproveCandidate(new CandidateApprove { CandidateId = 999, ProposalId = 1 });

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
            var candidate = new Candidate { CandidateId = 10, ProposalId = 1, UserId = 100, Message = "msg", EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(), Status = ProposalStatus.Pending };
            _context.Candidate.Add(candidate);
            await _context.SaveChangesAsync();

            var result = await _service.DisapproveCandidate(new CandidateApprove { CandidateId = 10, ProposalId = 1 });

            Assert.True(result.Success);
            Assert.Equal(ProposalStatus.Rejected, _context.Candidate.First().Status);
        }

        [Fact]
        public async Task DisapproveCandidate_CandidateNotFound_ReturnsFalse()
        {
            var result = await _service.DisapproveCandidate(new CandidateApprove { CandidateId = 999, ProposalId = 1 });

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
            var dto = new CandidateProposal { ProposalId = 5, UserId = 33, EstimatedDate = DateTime.UtcNow.AddDays(2).ToString(), ProposedPrice = 150, Message = "Test" };

            _emailMock.Setup(e => e.SendNewCandidateEmailAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _service.Candidate(dto);

            Assert.NotNull(result);
            Assert.Equal(33, result.UserId);
            _emailMock.Verify(e => e.SendNewCandidateEmailAsync(5, 33), Times.Once);
        }

        // -----------------------------
        // COUNTER PROPOSAL
        // -----------------------------
        [Fact]
        public async Task CounterProposal_CreatesAndSendsEmail()
        {
            var proposal = CreateValidProposal(proposalId: 7, ownerId: 99);
            _context.Proposal.Add(proposal);
            await _context.SaveChangesAsync();

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

            _emailMock.Setup(e => e.SendCounterProposalEmailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _service.CounterProposal(dto);

            Assert.True(result.Success);
            Assert.Single(_context.CounterProposal.ToList());
            _emailMock.Verify(e => e.SendCounterProposalEmailAsync(7, 12, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task CounterProposal_ProposalNotFound_ReturnsFalse()
        {
            var dto = new CounterProposalCreate { ProposalId = 999, FreelancerId = 1, CompanyId = 1, EstimatedDate = DateTime.UtcNow, ProposedPrice = 100, Message = "Test", IsAccepted = false, IsSendedByCompany = true };

            var result = await _service.CounterProposal(dto);

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
            _context.Proposal.Add(CreateValidProposal(proposalId: 1, ownerId: 10));
            _context.Proposal.Add(CreateValidProposal(proposalId: 2, ownerId: 20));
            await _context.SaveChangesAsync();

            var result = await _service.GetProposals(10);

            Assert.Single(result);
            Assert.Equal(10, result.First().OwnerId);
        }

        [Fact]
        public async Task GetAllProposals_ReturnsAvailableProposals()
        {
            _context.Proposal.Add(CreateValidProposal(proposalId: 1, isAvailable: true));
            _context.Proposal.Add(CreateValidProposal(proposalId: 2, isAvailable: false));
            await _context.SaveChangesAsync();

            var result = await _service.GetAllProposals();

            Assert.Single(result);
            Assert.True(result.First().IsAvailable);
        }

        [Fact]
        public async Task GetProposalById_ReturnsProposal()
        {
            _context.Proposal.Add(CreateValidProposal(proposalId: 1));
            await _context.SaveChangesAsync();

            var result = await _service.GetProposalById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProposalId);
        }

        [Fact]
        public async Task GetProposalByIdAndCandidate_ReturnsProposalWithCandidate()
        {
            var proposal = CreateValidProposal(proposalId: 1);
            proposal.Candidates.Add(new Candidate { CandidateId = 100, UserId = 5, ProposalId = 1, Message = "msg" });
            _context.Proposal.Add(proposal);
            await _context.SaveChangesAsync();

            var result = await _service.GetProposalByIdAndCandidate(1, 5);

            Assert.NotNull(result);
            Assert.Single(result.Candidates);
            Assert.Equal(5, result.Candidates.First().UserId);
        }

        [Fact]
        public async Task GetCounterProposalByProposalId_ReturnsCounters()
        {
            _context.CounterProposal.Add(CreateValidCounterProposal(counterId: 1, proposalId: 1));
            _context.CounterProposal.Add(CreateValidCounterProposal(counterId: 2, proposalId: 2));
            await _context.SaveChangesAsync();

            var result = await _service.GetCounterProposalByProposalId(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProposalId);
        }

        [Fact]
        public async Task GetProposalsByUserId_ReturnsProposals()
        {
            var proposal = CreateValidProposal(proposalId: 1);
            proposal.Candidates.Add(new Candidate { CandidateId = 1, UserId = 5 });
            _context.Proposal.Add(proposal);
            await _context.SaveChangesAsync();

            var result = await _service.GetProposalsByUserId(5);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProposalId);
        }

        [Fact]
        public async Task GetFreelancersToReview_ReturnsEmptyListWhenNone()
        {
            var result = await _service.GetFreelancersToReview(1);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCompaniesToReview_ReturnsEmptyListWhenNone()
        {
            var result = await _service.GetCompaniesToReview(1);
            Assert.Empty(result);
        }
    }
}
