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

        okResult.StatusCode.Should().Be(200);
        var data = Assert.IsType<List<Proposal>>(okResult.Value);
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetProposals_ShouldReturnNotFound_WhenNoProposals()
    {
        _proposalServiceMock.Setup(s => s.GetProposals(1)).ReturnsAsync((List<Proposal>)null);

        var actionResult = await _controller.GetProposals(1);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);

        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenProposalCreated()
    {
        var dto = new CreateProposal { Title = "Test", OwnerId = 1 };
        var proposal = new Proposal { ProposalId = 1, Title = "Test" };

        _proposalServiceMock.Setup(s => s.CreateProposal(dto)).ReturnsAsync(proposal);

        var actionResult = await _controller.Create(dto);
        var okResult = Assert.IsType<OkObjectResult>(actionResult);

        okResult.StatusCode.Should().Be(200);
        var data = ((dynamic)okResult.Value).proposal;
        ((int)data.ProposalId).Should().Be(1);
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
}
