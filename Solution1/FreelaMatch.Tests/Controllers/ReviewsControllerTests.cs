using System;
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

        var actionResult = await _controller.GetReviews(1);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        okResult.StatusCode.Should().Be(200);
        var data = Assert.IsType<List<Reviews>>(okResult.Value);
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnNotFound_WhenNoReviews()
    {
        _reviewsServiceMock.Setup(s => s.GetReviews(1))
            .ReturnsAsync(new List<Reviews>());

        var actionResult = await _controller.GetReviews(1);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);

        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetFreelancersToReview_ShouldReturnOk_WhenCandidatesExist()
    {
        _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
            .ReturnsAsync(new List<Candidate> { new Candidate { CandidateId = 1 } });

        var actionResult = await _controller.GetFreelancersToReview(1);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        okResult.StatusCode.Should().Be(200);
        var data = Assert.IsType<List<Candidate>>(okResult.Value);
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetCompaniesToReview_ShouldReturnOk_WhenCompaniesExist()
    {
        _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
            .ReturnsAsync(new List<Proposal> { new Proposal { ProposalId = 1 } });

        var actionResult = await _controller.GetCompaniesToReview(1);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        okResult.StatusCode.Should().Be(200);
        var data = Assert.IsType<List<Proposal>>(okResult.Value);
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnOk_WhenReviewCreated()
    {
        var dto = new ReviewCreate { ReviewerId = 1, ReceiverId = 2, ReviewText = "Great", Rating = 5, ProposalId = 1 };
        _reviewsServiceMock.Setup(s => s.CreateReview(dto))
            .ReturnsAsync(new Reviews { Id = 1, ReviewText = "Great" });

        var actionResult = await _controller.CreateReview(dto);
        var okResult = Assert.IsType<OkObjectResult>(actionResult);

        okResult.StatusCode.Should().Be(200);
        var review = ((dynamic)okResult.Value).review;
        Assert.Equal(1, review.Id);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnBadRequest_WhenThrowsException()
    {
        var dto = new ReviewCreate { ReviewerId = 1, ReceiverId = 2, ReviewText = "Great", Rating = 5, ProposalId = 1 };
        _reviewsServiceMock.Setup(s => s.CreateReview(dto))
            .ThrowsAsync(new InvalidOperationException("Error"));

        var actionResult = await _controller.CreateReview(dto);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);

        badRequestResult.StatusCode.Should().Be(400);
    }
}
