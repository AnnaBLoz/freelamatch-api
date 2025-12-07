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
using FreelaMatchAPI.Services;

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
            emailMock.Setup(e => e.SendApproveEmail(It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                IsAvailable = true,
                Title = "Test Proposal",
                Description = "Desc",
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(5),
                CreatedDate = DateTime.UtcNow
            });

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

            context.Candidate.Add(new Candidate
            {
                CandidateId = 10,
                ProposalId = 1,
                UserId = 100,
                Message = "msg",
                EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(),
                Status = ProposalStatus.Pending
            });
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
            emailMock.Setup(e => e.SendNewCandidateEmailAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            var dto = new CandidateProposal
            {
                ProposalId = 5,
                UserId = 33,
                EstimatedDate = DateTime.UtcNow.AddDays(2).ToString(),
                ProposedPrice = 150,
                Message = "Test"
            };

            context.Proposal.Add(new Proposal
            {
                ProposalId = 5,
                OwnerId = 99,
                Title = "Title",
                Description = "Desc",
                Price = 100
            });

            await context.SaveChangesAsync();

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
            emailMock.Setup(e => e.SendCounterProposalEmailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal
            {
                ProposalId = 7,
                OwnerId = 99,
                Title = "Title",
                Description = "Desc",
                Price = 500,
                MaxDate = DateTime.UtcNow.AddDays(5),
                CreatedDate = DateTime.UtcNow
            });
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
            Assert.Single(context.CounterProposal);

            emailMock.Verify(e =>
                e.SendCounterProposalEmailAsync(7, 12, It.IsAny<int>()),
                Times.Once);
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

            var dto = new CounterProposalCreate
            {
                ProposalId = 999,
                FreelancerId = 1,
                CompanyId = 1,
                EstimatedDate = DateTime.UtcNow,
                ProposedPrice = 100,
                Message = "Test",
                IsAccepted = false,
                IsSendedByCompany = true
            };

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

            context.Proposal.Add(new Proposal { ProposalId = 1, OwnerId = 10, IsAvailable = true, Title = "T", Description = "D" });
            context.Proposal.Add(new Proposal { ProposalId = 2, OwnerId = 20, IsAvailable = true, Title = "T2", Description = "D2" });
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

            context.Proposal.Add(new Proposal { ProposalId = 1, OwnerId = 1, IsAvailable = true, Title = "T1", Description = "D1" });
            context.Proposal.Add(new Proposal { ProposalId = 2, OwnerId = 2, IsAvailable = false, Title = "T2", Description = "D2" });
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

            context.Proposal.Add(new Proposal { ProposalId = 1, OwnerId = 1, Title = "T", Description = "D" });
            await context.SaveChangesAsync();

            var result = await service.GetProposalById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProposalId);
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

            context.Proposal.Add(new Proposal
            {
                ProposalId = 1,
                Title = "T",
                Description = "D",
                Candidates = new List<Candidate>
                {
                    new Candidate
                    {
                        UserId = 5,
                        CandidateId = 1,
                        EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(),
                        Message = "Test message",
                        Status = ProposalStatus.Pending
                    }
                }
            });
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

        // -----------------------------
        // GET PROPOSAL BY ID AND CANDIDATE
        // -----------------------------
        [Fact]
        public async Task GetProposalByIdAndCandidate_ReturnsProposalWithCandidate()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 1,
                Title = "T",
                Description = "D",
                IsAvailable = true,
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(5),
                CreatedDate = DateTime.UtcNow
            };

            context.Proposal.Add(proposal);
            await context.SaveChangesAsync();

            var candidate = new Candidate
            {
                CandidateId = 100,
                UserId = 5,
                ProposalId = 1,
                EstimatedDate = DateTime.UtcNow.AddDays(3).ToString(),
                Message = "Test message",
                Status = ProposalStatus.Pending
            };

            context.Candidate.Add(candidate);
            await context.SaveChangesAsync();

            // Limpar cache para forçar query real
            context.ChangeTracker.Clear();

            var result = await service.GetProposalByIdAndCandidate(1, 5);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProposalId);
        }

        [Fact]
        public async Task GetProposalByIdAndCandidate_ReturnsNull_WhenNotFound()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetProposalByIdAndCandidate(999, 5);

            Assert.Null(result);
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

            // Criar usuários necessários
            var freelancer = new User
            {
                Id = 1,
                Name = "Freelancer",
                Email = "freelancer@test.com",
                Password = "123",
                Token = "A"
            };

            var company = new User
            {
                Id = 10,
                Name = "Company",
                Email = "company@test.com",
                Password = "123",
                Token = "B"
            };

            context.Users.AddRange(freelancer, company);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                Title = "T",
                Description = "D",
                IsAvailable = true,
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(5),
                CreatedDate = DateTime.UtcNow
            };

            context.Proposal.Add(proposal);
            await context.SaveChangesAsync();

            context.CounterProposal.Add(new CounterProposal
            {
                CounterProposalId = 1,
                ProposalId = 1,
                Message = "Test message",
                FreelancerId = 1,
                CompanyId = 10,
                ProposedPrice = 100,
                EstimatedDate = DateTime.UtcNow,
                IsAccepted = false,
                IsSendedByCompany = false
            });

            await context.SaveChangesAsync();

            // Limpar cache
            context.ChangeTracker.Clear();

            var result = await service.GetCounterProposalByProposalId(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProposalId);
        }

        [Fact]
        public async Task GetCounterProposalByProposalId_ReturnsEmpty_WhenNone()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetCounterProposalByProposalId(999);

            Assert.Empty(result);
        }

        // -----------------------------
        // GET PROPOSAL BY ID - NOT FOUND
        // -----------------------------
        [Fact]
        public async Task GetProposalById_ReturnsNull_WhenNotFound()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetProposalById(999);

            Assert.Null(result);
        }

        // -----------------------------
        // GET ALL PROPOSALS - EMPTY
        // -----------------------------
        [Fact]
        public async Task GetAllProposals_ReturnsEmpty_WhenNoAvailableProposals()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal
            {
                ProposalId = 1,
                OwnerId = 1,
                IsAvailable = false, // Not available
                Title = "T",
                Description = "D"
            });
            await context.SaveChangesAsync();

            var result = await service.GetAllProposals();

            Assert.Empty(result);
        }

        // -----------------------------
        // GET PROPOSALS - EMPTY
        // -----------------------------
        [Fact]
        public async Task GetProposals_ReturnsEmpty_WhenCompanyHasNoProposals()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal { ProposalId = 1, OwnerId = 10, Title = "T", Description = "D" });
            await context.SaveChangesAsync();

            var result = await service.GetProposals(999); // Different company

            Assert.Empty(result);
        }

        // -----------------------------
        // GET PROPOSALS BY USER ID - EMPTY
        // -----------------------------
        [Fact]
        public async Task GetProposalsByUserId_ReturnsEmpty_WhenUserHasNoCandidates()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var result = await service.GetProposalsByUserId(999);

            Assert.Empty(result);
        }

        // -----------------------------
        // CREATE PROPOSAL - WITHOUT SKILLS
        // -----------------------------
        [Fact]
        public async Task CreateProposal_CreatesProposal_WithoutSkills()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var dto = new CreateProposal
            {
                Title = "Simple Proposal",
                Description = "Description",
                Price = 500,
                MaxDate = DateTime.UtcNow.AddDays(10),
                OwnerId = 15,
                RequiredSkills = new List<ProposalSkillCreate>() // Empty list
            };

            var result = await service.CreateProposal(dto);

            Assert.NotNull(result);
            Assert.Equal("Simple Proposal", result.Title);
            Assert.Equal(15, result.OwnerId);
            Assert.True(result.IsAvailable);
            Assert.Empty(context.ProposalSkill);
        }

        // -----------------------------
        // APPROVE CANDIDATE - WITH PROPOSAL UPDATE
        // -----------------------------
        [Fact]
        public async Task ApproveCandidate_SetsProposalUnavailable()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendApproveEmail(It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                IsAvailable = true,
                Title = "Test",
                Description = "Desc",
                Price = 100
            };

            context.Proposal.Add(proposal);

            context.Candidate.Add(new Candidate
            {
                CandidateId = 1,
                ProposalId = 1,
                UserId = 100,
                Message = "msg",
                EstimatedDate = DateTime.UtcNow.ToString(),
                Status = ProposalStatus.Pending
            });

            await context.SaveChangesAsync();

            await service.ApproveCandidate(new CandidateApprove { CandidateId = 1, ProposalId = 1 });

            var updatedProposal = await context.Proposal.FindAsync(1);
            Assert.False(updatedProposal.IsAvailable);
        }

        // -----------------------------
        // GET FREELANCERS TO REVIEW - WITH DATA
        // -----------------------------
        [Fact]
        public async Task GetFreelancersToReview_ReturnsFreelancers_WhenConditionsMet()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var user = new User
            {
                Id = 1,
                Name = "Freelancer",
                Email = "freelancer@test.com",
                Password = "123",
                Token = "A"
            };

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                Title = "Test",
                Description = "Desc",
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(-5), // Past date
                IsAvailable = false
            };

            var candidate = new Candidate
            {
                CandidateId = 1,
                ProposalId = 1,
                UserId = 1,
                Status = ProposalStatus.Accepted,
                Message = "msg",
                EstimatedDate = DateTime.UtcNow.ToString()
            };

            context.Users.Add(user);
            context.Proposal.Add(proposal);
            context.Candidate.Add(candidate);
            await context.SaveChangesAsync();

            var result = await service.GetFreelancersToReview(10);

            Assert.Single(result);
            Assert.Equal(1, result.First().UserId);
        }

        // -----------------------------
        // GET COMPANIES TO REVIEW - WITH DATA
        // -----------------------------
        [Fact]
        public async Task GetCompaniesToReview_ReturnsCompanies_WhenConditionsMet()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var owner = new User
            {
                Id = 10,
                Name = "Company",
                Email = "company@test.com",
                Password = "123",
                Token = "A"
            };

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                Title = "Test",
                Description = "Desc",
                Price = 100,
                MaxDate = DateTime.UtcNow.AddDays(-5), // Past date
                IsAvailable = false,
                Owner = owner
            };

            var candidate = new Candidate
            {
                CandidateId = 1,
                ProposalId = 1,
                UserId = 5,
                Status = ProposalStatus.Accepted, // Not reviewed
                Message = "msg",
                EstimatedDate = DateTime.UtcNow.ToString()
            };

            context.Users.Add(owner);
            context.Proposal.Add(proposal);
            context.Candidate.Add(candidate);
            await context.SaveChangesAsync();

            var result = await service.GetCompaniesToReview(5);

            Assert.Single(result);
            Assert.Equal(10, result.First().OwnerId);
        }

        // -----------------------------
        // COUNTER PROPOSAL - WITH ALL FIELDS
        // -----------------------------
        [Fact]
        public async Task CounterProposal_CreatesWithAllFields()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendCounterProposalEmailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal
            {
                ProposalId = 5,
                OwnerId = 20,
                Title = "Title",
                Description = "Desc",
                Price = 1000
            });
            await context.SaveChangesAsync();

            var dto = new CounterProposalCreate
            {
                ProposalId = 5,
                FreelancerId = 3,
                CompanyId = 20,
                EstimatedDate = DateTime.UtcNow.AddDays(15),
                ProposedPrice = 1200,
                Message = "I need more",
                IsAccepted = true,
                IsSendedByCompany = false
            };

            var result = await service.CounterProposal(dto);

            Assert.True(result.Success);

            var counter = await context.CounterProposal.FirstAsync();
            Assert.Equal(5, counter.ProposalId);
            Assert.Equal(3, counter.FreelancerId);
            Assert.Equal(20, counter.CompanyId);
            Assert.Equal(1200, counter.ProposedPrice);
            Assert.True(counter.IsAccepted);
            Assert.False(counter.IsSendedByCompany);
        }

        // -----------------------------
        // CANDIDATE - VERIFY ALL PROPERTIES
        // -----------------------------
        [Fact]
        public async Task Candidate_SetsAllPropertiesCorrectly()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(e => e.SendNewCandidateEmailAsync(It.IsAny<int>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var service = new ProposalService(context, emailMock.Object);

            context.Proposal.Add(new Proposal
            {
                ProposalId = 10,
                OwnerId = 50,
                Title = "Job",
                Description = "Desc",
                Price = 2000
            });
            await context.SaveChangesAsync();

            var estimatedDate = DateTime.UtcNow.AddDays(20).ToString();
            var dto = new CandidateProposal
            {
                ProposalId = 10,
                UserId = 77,
                EstimatedDate = estimatedDate,
                ProposedPrice = 1800,
                Message = "I'm interested"
            };

            var result = await service.Candidate(dto);

            Assert.Equal(10, result.ProposalId);
            Assert.Equal(77, result.UserId);
            Assert.Equal(ProposalStatus.Pending, result.Status);
            Assert.Equal(estimatedDate, result.EstimatedDate);
            Assert.Equal(1800, result.ProposedPrice);
            Assert.Equal("I'm interested", result.Message);
            Assert.True(result.AppliedAt <= DateTime.UtcNow);
        }

        // -----------------------------
        // GET ALL PROPOSALS - WITH INCLUDES
        // -----------------------------
        [Fact]
        public async Task GetAllProposals_IncludesRequiredSkillsAndCandidates()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var skill = new Skill { SkillId = 1, Name = "C#" };
            context.Skills.Add(skill);

            var user = new User
            {
                Id = 1,
                Name = "User",
                Email = "user@test.com",
                Password = "123",
                Token = "A"
            };
            context.Users.Add(user);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 10,
                IsAvailable = true,
                Title = "Job",
                Description = "Desc",
                Price = 100
            };
            context.Proposal.Add(proposal);

            await context.SaveChangesAsync();

            context.ProposalSkill.Add(new ProposalSkill
            {
                ProposalId = 1,
                SkillId = 1,
                IsActive = true
            });

            context.Candidate.Add(new Candidate
            {
                ProposalId = 1,
                UserId = 1,
                Message = "msg",
                EstimatedDate = DateTime.UtcNow.ToString(),
                Status = ProposalStatus.Pending
            });

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await service.GetAllProposals();

            Assert.Single(result);
            var returnedProposal = result.First();
            Assert.NotNull(returnedProposal.RequiredSkills);
            Assert.NotNull(returnedProposal.Candidates);
        }

        // -----------------------------
        // GET PROPOSALS - WITH INCLUDES
        // -----------------------------
        [Fact]
        public async Task GetProposals_IncludesRequiredSkillsAndCandidates()
        {
            var context = GetDbContext();
            var emailMock = new Mock<IEmailService>();
            var service = new ProposalService(context, emailMock.Object);

            var skill = new Skill { SkillId = 1, Name = "Java" };
            context.Skills.Add(skill);

            var proposal = new Proposal
            {
                ProposalId = 1,
                OwnerId = 15,
                IsAvailable = true,
                Title = "Job",
                Description = "Desc",
                Price = 100
            };
            context.Proposal.Add(proposal);

            await context.SaveChangesAsync();

            context.ProposalSkill.Add(new ProposalSkill
            {
                ProposalId = 1,
                SkillId = 1,
                IsActive = true
            });

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await service.GetProposals(15);

            Assert.Single(result);
            Assert.NotNull(result.First().RequiredSkills);
        }
    }
}
