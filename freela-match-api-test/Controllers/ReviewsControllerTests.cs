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
            var createdReview = new Reviews { Id = 1, Rating = 5 };

            _reviewsServiceMock.Setup(s => s.CreateReview(dto))
                .ReturnsAsync(createdReview);

            var result = await _controller.CreateReview(dto);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            Assert.NotNull(ok);
            Assert.NotNull(ok.Value);

            // Usar reflexão para acessar propriedades de objetos anônimos
            var valueType = ok.Value.GetType();
            var reviewProperty = valueType.GetProperty("review");
            Assert.NotNull(reviewProperty);

            var review = reviewProperty.GetValue(ok.Value) as Reviews;
            Assert.NotNull(review);
            Assert.Equal(1, review.Id);
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

        // ============================================================
        // GET REVIEWS - Cenários Adicionais
        // ============================================================

        [Fact]
        public async Task GetReviews_ShouldReturnNotFound_WhenNull()
        {
            _reviewsServiceMock.Setup(s => s.GetReviews(1))
                .ReturnsAsync((List<Reviews>)null);

            var result = await _controller.GetReviews(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;
            notFound.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetReviews_ShouldReturnOk_WithMultipleReviews()
        {
            var reviews = new List<Reviews>
    {
        new Reviews { Id = 1, ReviewText = "Test 1", Rating = 5 },
        new Reviews { Id = 2, ReviewText = "Test 2", Rating = 4 },
        new Reviews { Id = 3, ReviewText = "Test 3", Rating = 3 }
    };

            _reviewsServiceMock.Setup(s => s.GetReviews(1))
                .ReturnsAsync(reviews);

            var result = await _controller.GetReviews(1);

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;
            var data = ok!.Value.Should().BeAssignableTo<List<Reviews>>().Subject;
            data.Count.Should().Be(3);
        }

        // ============================================================
        // GET FREELANCERS TO REVIEW - Cenários Adicionais
        // ============================================================

        [Fact]
        public async Task GetFreelancersToReview_ShouldReturnNotFound_WhenNull()
        {
            _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
                .ReturnsAsync((List<Candidate>)null);

            var result = await _controller.GetFreelancersToReview(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetFreelancersToReview_ShouldReturnNotFound_WhenEmpty()
        {
            _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
                .ReturnsAsync(new List<Candidate>());

            var result = await _controller.GetFreelancersToReview(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;
            notFound.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetFreelancersToReview_ShouldReturnOk_WithMultipleCandidates()
        {
            var candidates = new List<Candidate>
    {
        new Candidate { CandidateId = 1, UserId = 10 },
        new Candidate { CandidateId = 2, UserId = 20 }
    };

            _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
                .ReturnsAsync(candidates);

            var result = await _controller.GetFreelancersToReview(1);

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;
            var data = ok!.Value.Should().BeAssignableTo<List<Candidate>>().Subject;
            data.Count.Should().Be(2);
        }

        // ============================================================
        // GET COMPANIES TO REVIEW - Cenários Adicionais
        // ============================================================

        [Fact]
        public async Task GetCompaniesToReview_ShouldReturnNotFound_WhenNull()
        {
            _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
                .ReturnsAsync((List<Proposal>)null);

            var result = await _controller.GetCompaniesToReview(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetCompaniesToReview_ShouldReturnNotFound_WhenEmpty()
        {
            _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
                .ReturnsAsync(new List<Proposal>());

            var result = await _controller.GetCompaniesToReview(1);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result.Result as NotFoundObjectResult;
            notFound.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetCompaniesToReview_ShouldReturnOk_WithMultipleProposals()
        {
            var proposals = new List<Proposal>
    {
        new Proposal { ProposalId = 1, Title = "Proposal 1" },
        new Proposal { ProposalId = 2, Title = "Proposal 2" }
    };

            _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
                .ReturnsAsync(proposals);

            var result = await _controller.GetCompaniesToReview(1);

            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;
            var data = ok!.Value.Should().BeAssignableTo<List<Proposal>>().Subject;
            data.Count.Should().Be(2);
        }

        // ============================================================
        // CREATE REVIEW - Cenários Adicionais
        // ============================================================

        [Fact]
        public async Task CreateReview_ShouldReturnOk_WithAllReviewData()
        {
            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                Rating = 5,
                ReviewText = "Excellent work!",
                ProposalId = 10
            };

            var createdReview = new Reviews
            {
                Id = 1,
                ReviewerId = 1,
                ReceiverId = 2,
                Rating = 5,
                ReviewText = "Excellent work!",
                ProposalId = 10
            };

            _reviewsServiceMock.Setup(s => s.CreateReview(dto))
                .ReturnsAsync(createdReview);

            var result = await _controller.CreateReview(dto);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            Assert.NotNull(ok);

            var valueType = ok.Value.GetType();
            var reviewProperty = valueType.GetProperty("review");
            var review = reviewProperty.GetValue(ok.Value) as Reviews;

            Assert.NotNull(review);
            Assert.Equal(1, review.Id);
            Assert.Equal(5, review.Rating);
            Assert.Equal("Excellent work!", review.ReviewText);
        }

        [Fact]
        public async Task CreateReview_ShouldReturnBadRequest_WithExceptionMessage()
        {
            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ProposalId = 1,
                Rating = 5
            };

            var exceptionMessage = "Review already exists";

            _reviewsServiceMock.Setup(s => s.CreateReview(dto))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var result = await _controller.CreateReview(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest.StatusCode.Should().Be(400);

            // Verificar a mensagem de erro
            var valueType = badRequest.Value.GetType();
            var messageProperty = valueType.GetProperty("message");
            var message = messageProperty.GetValue(badRequest.Value) as string;
            Assert.Equal(exceptionMessage, message);
        }

        [Fact]
        public async Task CreateReview_ShouldCallServiceWithCorrectParameters()
        {
            var dto = new ReviewCreate
            {
                ReviewerId = 10,
                ReceiverId = 20,
                Rating = 4,
                ReviewText = "Good",
                ProposalId = 99
            };

            var createdReview = new Reviews { Id = 1 };

            _reviewsServiceMock.Setup(s => s.CreateReview(It.IsAny<ReviewCreate>()))
                .ReturnsAsync(createdReview);

            await _controller.CreateReview(dto);

            _reviewsServiceMock.Verify(s => s.CreateReview(
                It.Is<ReviewCreate>(r =>
                    r.ReviewerId == 10 &&
                    r.ReceiverId == 20 &&
                    r.Rating == 4 &&
                    r.ReviewText == "Good" &&
                    r.ProposalId == 99
                )
            ), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task CreateReview_ShouldAcceptDifferentRatings(int rating)
        {
            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                Rating = rating,
                ProposalId = 1
            };

            var createdReview = new Reviews { Id = 1, Rating = rating };

            _reviewsServiceMock.Setup(s => s.CreateReview(dto))
                .ReturnsAsync(createdReview);

            var result = await _controller.CreateReview(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        // ============================================================
        // TESTES DE INTEGRAÇÃO DOS MOCKS
        // ============================================================

        [Fact]
        public async Task GetReviews_ShouldNotCallOtherServices()
        {
            _reviewsServiceMock.Setup(s => s.GetReviews(1))
                .ReturnsAsync(new List<Reviews> { new Reviews { Id = 1 } });

            await _controller.GetReviews(1);

            // Verificar que apenas o ReviewsService foi chamado
            _reviewsServiceMock.Verify(s => s.GetReviews(1), Times.Once);
            _proposalServiceMock.Verify(s => s.GetFreelancersToReview(It.IsAny<int>()), Times.Never);
            _proposalServiceMock.Verify(s => s.GetCompaniesToReview(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetFreelancersToReview_ShouldCallProposalService()
        {
            _proposalServiceMock.Setup(s => s.GetFreelancersToReview(1))
                .ReturnsAsync(new List<Candidate> { new Candidate { CandidateId = 1 } });

            await _controller.GetFreelancersToReview(1);

            _proposalServiceMock.Verify(s => s.GetFreelancersToReview(1), Times.Once);
            _reviewsServiceMock.Verify(s => s.GetReviews(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCompaniesToReview_ShouldCallProposalService()
        {
            _proposalServiceMock.Setup(s => s.GetCompaniesToReview(1))
                .ReturnsAsync(new List<Proposal> { new Proposal { ProposalId = 1 } });

            await _controller.GetCompaniesToReview(1);

            _proposalServiceMock.Verify(s => s.GetCompaniesToReview(1), Times.Once);
            _reviewsServiceMock.Verify(s => s.GetReviews(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateReview_ShouldCallServiceOnce()
        {
            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                Rating = 5,
                ProposalId = 1
            };

            var createdReview = new Reviews { Id = 1 };

            _reviewsServiceMock.Setup(s => s.CreateReview(dto))
                .ReturnsAsync(createdReview);

            await _controller.CreateReview(dto);

            _reviewsServiceMock.Verify(s => s.CreateReview(It.IsAny<ReviewCreate>()), Times.Once);
        }

        // ============================================================
        // TESTES DE DIFERENTES USERIDS
        // ============================================================

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task GetReviews_ShouldWorkWithDifferentUserIds(int userId)
        {
            _reviewsServiceMock.Setup(s => s.GetReviews(userId))
                .ReturnsAsync(new List<Reviews> { new Reviews { Id = 1 } });

            var result = await _controller.GetReviews(userId);

            result.Result.Should().BeOfType<OkObjectResult>();
            _reviewsServiceMock.Verify(s => s.GetReviews(userId), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task GetFreelancersToReview_ShouldWorkWithDifferentUserIds(int userId)
        {
            _proposalServiceMock.Setup(s => s.GetFreelancersToReview(userId))
                .ReturnsAsync(new List<Candidate> { new Candidate { CandidateId = 1 } });

            var result = await _controller.GetFreelancersToReview(userId);

            result.Result.Should().BeOfType<OkObjectResult>();
            _proposalServiceMock.Verify(s => s.GetFreelancersToReview(userId), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task GetCompaniesToReview_ShouldWorkWithDifferentUserIds(int userId)
        {
            _proposalServiceMock.Setup(s => s.GetCompaniesToReview(userId))
                .ReturnsAsync(new List<Proposal> { new Proposal { ProposalId = 1 } });

            var result = await _controller.GetCompaniesToReview(userId);

            result.Result.Should().BeOfType<OkObjectResult>();
            _proposalServiceMock.Verify(s => s.GetCompaniesToReview(userId), Times.Once);
        }
    }
}