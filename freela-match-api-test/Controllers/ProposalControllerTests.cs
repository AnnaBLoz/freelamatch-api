using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;

namespace freela_match_api_test.Controllers
{
    public class ProposalControllerTests
    {
        private readonly Mock<IProposalService> _proposalServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly ProposalController _controller;

        public ProposalControllerTests()
        {
            _proposalServiceMock = new Mock<IProposalService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new ProposalController(_proposalServiceMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task GetProposals_ShouldReturnOk_WhenProposalsExist()
        {
            _proposalServiceMock.Setup(s => s.GetProposals(1))
                .ReturnsAsync(new List<Proposal> { new Proposal { ProposalId = 1, Title = "Test" } });

            var actionResult = await _controller.GetProposals(1);

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<Proposal>>(okResult.Value);

            data.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetProposals_ShouldReturnNotFound_WhenNoProposals()
        {
            _proposalServiceMock.Setup(s => s.GetProposals(1)).ReturnsAsync((List<Proposal>)null);

            var actionResult = await _controller.GetProposals(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            notFound.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenProposalCreated()
        {
            var dto = new CreateProposal { Title = "Test", OwnerId = 1 };
            var createdProposal = new Proposal { ProposalId = 1, Title = "Test" };

            _proposalServiceMock.Setup(s => s.CreateProposal(dto)).ReturnsAsync(createdProposal);

            var result = await _controller.Create(dto);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            Assert.NotNull(ok);
            Assert.NotNull(ok.Value);

            // Usar reflexão para acessar propriedades de objetos anônimos
            var valueType = ok.Value.GetType();
            var proposalProperty = valueType.GetProperty("proposal");
            Assert.NotNull(proposalProperty);

            var proposal = proposalProperty.GetValue(ok.Value) as Proposal;
            Assert.NotNull(proposal);
            Assert.Equal(1, proposal.ProposalId);
        }

        [Fact]
        public async Task ApproveCandidate_ShouldReturnOk_WhenSuccess()
        {
            var dto = new CandidateApprove { CandidateId = 1, ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.ApproveCandidate(dto))
                .ReturnsAsync((true, "ok", new Candidate { CandidateId = 1 }));

            var actionResult = await _controller.ApproveCandidate(dto);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DisapproveCandidate_ShouldReturnOk_WhenSuccess()
        {
            var dto = new CandidateApprove { CandidateId = 1, ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.DisapproveCandidate(dto))
                .ReturnsAsync((true, "ok", new Candidate { CandidateId = 1 }));

            var actionResult = await _controller.DisapproveCandidate(dto);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CounterProposal_ShouldReturnOk_WhenSuccess()
        {
            var dto = new CounterProposalCreate { ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.CounterProposal(dto))
                .ReturnsAsync((true, "ok", new Proposal { ProposalId = 1 }));

            var actionResult = await _controller.CounterProposal(dto);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Candidate_ShouldReturnOk_WhenSuccess()
        {
            var dto = new CandidateProposal { ProposalId = 1, UserId = 1 };

            _proposalServiceMock.Setup(s => s.Candidate(dto))
                .ReturnsAsync(new Candidate { CandidateId = 1 });

            var actionResult = await _controller.Candidate(dto);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAllProposals_ShouldReturnOk_WithProposalsList()
        {
            var proposals = new List<Proposal>
    {
        new Proposal { ProposalId = 1 },
        new Proposal { ProposalId = 2 }
    };

            _proposalServiceMock.Setup(s => s.GetAllProposals())
                .ReturnsAsync(proposals);

            var result = await _controller.GetAllProposals();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<Proposal>>(okResult.Value);
            data.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetProposalById_ShouldReturnOk_WhenProposalExists()
        {
            var proposal = new Proposal { ProposalId = 1, Title = "Test" };

            _proposalServiceMock.Setup(s => s.GetProposalById(1))
                .ReturnsAsync(proposal);

            var result = await _controller.GetProposalById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<Proposal>(okResult.Value);
            data.ProposalId.Should().Be(1);
        }

        [Fact]
        public async Task GetProposalById_ShouldReturnNotFound_WhenProposalDoesNotExist()
        {
            _proposalServiceMock.Setup(s => s.GetProposalById(1))
                .ReturnsAsync((Proposal)null);

            var result = await _controller.GetProposalById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetProposalByIdAndCandidate_ShouldReturnOk_WhenFound()
        {
            var proposal = new Proposal { ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.GetProposalByIdAndCandidate(1, 1))
                .ReturnsAsync(proposal);

            var result = await _controller.GetProposalByIdAndCandidate(1, 1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProposalByIdAndCandidate_ShouldReturnNotFound_WhenNotFound()
        {
            _proposalServiceMock.Setup(s => s.GetProposalByIdAndCandidate(1, 1))
                .ReturnsAsync((Proposal)null);

            var result = await _controller.GetProposalByIdAndCandidate(1, 1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenInvalidOperationException()
        {
            var dto = new CreateProposal { Title = "Test" };

            _proposalServiceMock.Setup(s => s.CreateProposal(dto))
                .ThrowsAsync(new InvalidOperationException("Error message"));

            var result = await _controller.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ApproveCandidate_ShouldReturnBadRequest_WhenFails()
        {
            var dto = new CandidateApprove { CandidateId = 1, ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.ApproveCandidate(dto))
                .ReturnsAsync((false, "Error occurred", null));

            var result = await _controller.ApproveCandidate(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task DisapproveCandidate_ShouldReturnBadRequest_WhenFails()
        {
            var dto = new CandidateApprove { CandidateId = 1, ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.DisapproveCandidate(dto))
                .ReturnsAsync((false, "Error occurred", null));

            var result = await _controller.DisapproveCandidate(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CounterProposal_ShouldReturnBadRequest_WhenFails()
        {
            var dto = new CounterProposalCreate { ProposalId = 1 };

            _proposalServiceMock.Setup(s => s.CounterProposal(dto))
                .ReturnsAsync((false, "Error message", null));

            var result = await _controller.CounterProposal(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetCounterProposalByProposalId_ShouldReturnOk_WhenFound()
        {
            var counterProposals = new List<CounterProposal>
    {
        new CounterProposal { ProposalId = 1 }
    };

            _proposalServiceMock.Setup(s => s.GetCounterProposalByProposalId(1))
                .ReturnsAsync(counterProposals);

            var result = await _controller.GetCounterProposalByProposalId(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCounterProposalByProposalId_ShouldReturnNotFound_WhenNull()
        {
            _proposalServiceMock.Setup(s => s.GetCounterProposalByProposalId(1))
                .ReturnsAsync((List<CounterProposal>)null);

            var result = await _controller.GetCounterProposalByProposalId(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProposalsByUserId_ShouldReturnOk_WhenFound()
        {
            var proposals = new List<Proposal> { new Proposal { ProposalId = 1 } };

            _proposalServiceMock.Setup(s => s.GetProposalsByUserId(1))
                .ReturnsAsync(proposals);

            var result = await _controller.GetProposalsByUserId(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProposalsByUserId_ShouldReturnNotFound_WhenNull()
        {
            _proposalServiceMock.Setup(s => s.GetProposalsByUserId(1))
                .ReturnsAsync((List<Proposal>)null);

            var result = await _controller.GetProposalsByUserId(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}