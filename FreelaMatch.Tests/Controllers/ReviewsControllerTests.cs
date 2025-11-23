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

public class ReviewsControllerTests
{
    private readonly Mock<IReviewsService> _reviewsServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IProposalService> _proposalServiceMock;
    private readonly ReviewsController _controller;

    public ReviewsControllerTests()
    {
        _reviewsServiceMock = new Mock<IReviewsService>();
        _userServiceMock = new Mock<IUserService>();
        _proposalServiceMock = new Mock<IProposalService>();

        _controller = new ReviewsController(
            _reviewsServiceMock.Object,
            _userServiceMock.Object,
            _proposalServiceMock.Object
        );
    }

    [Fact]
    public async Task GetReviews_ShouldReturnOk_WhenReviewsExist()
    {
        _reviewsServiceMock.Setup(s => s.GetReviews(1))
            .ReturnsAsync(new List<Reviews> { new Reviews { Id = 1, ReviewText = "Test" } });

        var result = await _controller.GetReviews(1);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;

        var data = ok!.Value.Should().BeAssignableTo<List<Reviews>>().Subject;
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnNotFound_WhenEmpty()
    {
        _reviewsServiceMock.Setup(s => s.GetReviews(1)).ReturnsAsync(new List<Reviews>());

        var result = await _controller.GetReviews(1);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetFreelancersToReview_ShouldReturnOk_WhenFound()
    {
        _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
            .ReturnsAsync(new List<Candidate> { new Candidate { CandidateId = 1 } });

        var result = await _controller.GetFreelancersToReview(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCompaniesToReview_ShouldReturnOk_WhenFound()
    {
        _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
            .ReturnsAsync(new List<Proposal> { new Proposal { ProposalId = 1 } });

        var result = await _controller.GetCompaniesToReview(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateReview_ShouldReturnOk_WhenSuccess()
    {
        var dto = new ReviewCreate { ReviewerId = 1, ReceiverId = 2, Rating = 5, ProposalId = 1 };

        _reviewsServiceMock.Setup(s => s.CreateReview(dto))
            .ReturnsAsync(new Reviews { Id = 1 });

        var result = await _controller.CreateReview(dto);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;

        ((dynamic)ok!.Value).review.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnBadRequest_WhenException()
    {
        var dto = new ReviewCreate { ReviewerId = 1, ReceiverId = 2, ProposalId = 1 };

        _reviewsServiceMock.Setup(s => s.CreateReview(dto))
            .ThrowsAsync(new InvalidOperationException("Error"));

        var result = await _controller.CreateReview(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
