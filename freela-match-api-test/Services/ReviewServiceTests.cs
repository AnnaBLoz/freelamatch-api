using FreelaMatchAPI.Data;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace freela_match_api_test.Services
{
    public class ReviewsServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ReviewsService GetService(AppDbContext context)
        {
            return new ReviewsService(context);
        }

        // ============================================================
        // GET REVIEWS
        // ============================================================
        [Fact]
        public async Task GetReviews_ShouldReturnReviewsForUser()
        {
            var context = GetDbContext();

            // -------- CRIA USERS NECESSÁRIOS PARA OS INCLUDE --------
            context.Users.AddRange(
                new User { Id = 10, Name = "User 10", Email = "u10@test.com", Password = "123", Token = "A" },
                new User { Id = 20, Name = "User 20", Email = "u20@test.com", Password = "123", Token = "A" },
                new User { Id = 30, Name = "User 30", Email = "u30@test.com", Password = "123", Token = "A" },
                new User { Id = 77, Name = "User 77", Email = "u77@test.com", Password = "123", Token = "A" },
                new User { Id = 99, Name = "User 99", Email = "u99@test.com", Password = "123", Token = "A" }
            );
            await context.SaveChangesAsync();

            // -------- CRIA AS REVIEWS --------
            context.Reviews.Add(new Reviews
            {
                Id = 1,
                ReviewerId = 10,
                ReceiverId = 20
            });

            context.Reviews.Add(new Reviews
            {
                Id = 2,
                ReviewerId = 30,
                ReceiverId = 10
            });

            context.Reviews.Add(new Reviews
            {
                Id = 3,
                ReviewerId = 99,
                ReceiverId = 77
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            // -------- EXECUTA --------
            var result = await service.GetReviews(10);

            // -------- ASSERTS --------
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }

        // ============================================================
        // CREATE REVIEW - CASO NORMAL
        // ============================================================
        [Fact]
        public async Task CreateReview_ShouldCreateReview()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Ótimo trabalho",
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.NotNull(result);
            Assert.Equal("Ótimo trabalho", result.ReviewText);
            Assert.Equal(5, result.Rating);

            var reviewInDb = await context.Reviews.FirstOrDefaultAsync();
            Assert.NotNull(reviewInDb);
        }

        // ============================================================
        // CREATE REVIEW - ATUALIZAÇÃO DO CANDIDATE
        // ============================================================
        [Fact]
        public async Task CreateReview_ShouldUpdateCandidateStatus_WhenAcceptedCandidateExists()
        {
            var context = GetDbContext();

            context.Candidate.Add(new Candidate
            {
                CandidateId = 50,
                UserId = 2,
                ProposalId = 99,
                Status = ProposalStatus.Accepted,

                // CAMPOS OBRIGATÓRIOS DO MODELO
                EstimatedDate = DateTime.UtcNow.ToString(),
                Message = "Teste"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Bom trabalho",
                Rating = 4,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            var candidate = await context.Candidate.FirstAsync();

            Assert.Equal(ProposalStatus.Reviewed, candidate.Status);
        }

        // ============================================================
        // CREATE REVIEW - NÃO ATUALIZAR CANDIDATE
        // ============================================================
        [Fact]
        public async Task CreateReview_ShouldNotUpdateCandidate_WhenNoAcceptedCandidateExists()
        {
            var context = GetDbContext();

            context.Candidate.Add(new Candidate
            {
                CandidateId = 50,
                UserId = 2,
                ProposalId = 99,
                Status = ProposalStatus.Pending,

                // CAMPOS OBRIGATÓRIOS DO MODELO
                EstimatedDate = DateTime.UtcNow.ToString(),
                Message = "Teste"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Avaliação",
                Rating = 3,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            var candidate = await context.Candidate.FirstAsync();

            Assert.Equal(ProposalStatus.Pending, candidate.Status);
        }

        [Fact]
        public async Task GetReviews_ShouldReturnEmptyList_WhenNoReviewsExist()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetReviews(999);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReviews_ShouldReturnOnlyReviewsWhereUserIsReceiver()
        {
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "User 1", Email = "u1@test.com", Password = "123", Token = "A" },
                new User { Id = 2, Name = "User 2", Email = "u2@test.com", Password = "123", Token = "A" }
            );
            await context.SaveChangesAsync();

            context.Reviews.Add(new Reviews
            {
                Id = 1,
                ReviewerId = 2,
                ReceiverId = 1,
                ReviewText = "Good",
                Rating = 5
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetReviews(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].ReceiverId);
        }

        [Fact]
        public async Task GetReviews_ShouldReturnOnlyReviewsWhereUserIsReviewer()
        {
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "User 1", Email = "u1@test.com", Password = "123", Token = "A" },
                new User { Id = 2, Name = "User 2", Email = "u2@test.com", Password = "123", Token = "A" }
            );
            await context.SaveChangesAsync();

            context.Reviews.Add(new Reviews
            {
                Id = 1,
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Good",
                Rating = 5
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetReviews(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].ReviewerId);
        }

        [Fact]
        public async Task GetReviews_ShouldIncludeReviewerAndReceiverNavigationProperties()
        {
            var context = GetDbContext();

            context.Users.AddRange(
                new User { Id = 1, Name = "Reviewer Name", Email = "reviewer@test.com", Password = "123", Token = "A" },
                new User { Id = 2, Name = "Receiver Name", Email = "receiver@test.com", Password = "123", Token = "A" }
            );
            await context.SaveChangesAsync();

            context.Reviews.Add(new Reviews
            {
                Id = 1,
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test",
                Rating = 5
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetReviews(1);

            Assert.Single(result);
            Assert.NotNull(result[0].Reviewer);
            Assert.NotNull(result[0].Receiver);
            Assert.Equal("Reviewer Name", result[0].Reviewer.Name);
            Assert.Equal("Receiver Name", result[0].Receiver.Name);
        }

        [Fact]
        public async Task CreateReview_ShouldSetCreatedAtTimestamp()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var beforeCreate = DateTime.UtcNow;

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test",
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            var afterCreate = DateTime.UtcNow;

            Assert.NotNull(result.CreatedAt);
            Assert.True(result.CreatedAt >= beforeCreate);
            Assert.True(result.CreatedAt <= afterCreate);
        }

        [Fact]
        public async Task CreateReview_ShouldSetAllFields_Correctly()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 10,
                ReceiverId = 20,
                ReviewText = "Excelente profissional",
                Rating = 5,
                ProposalId = 123
            };

            var result = await service.CreateReview(dto);

            Assert.NotNull(result);
            Assert.Equal(10, result.ReviewerId);
            Assert.Equal(20, result.ReceiverId);
            Assert.Equal("Excelente profissional", result.ReviewText);
            Assert.Equal(5, result.Rating);
            Assert.Equal(123, result.ProposalId);
        }

        [Fact]
        public async Task CreateReview_ShouldPersistToDatabase()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test",
                Rating = 4,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            var reviewInDb = await context.Reviews.FindAsync(result.Id);

            Assert.NotNull(reviewInDb);
            Assert.Equal(result.Id, reviewInDb.Id);
            Assert.Equal("Test", reviewInDb.ReviewText);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task CreateReview_ShouldAcceptDifferentRatings(int rating)
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test",
                Rating = rating,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.Equal(rating, result.Rating);
        }

        [Fact]
        public async Task CreateReview_ShouldHandleEmptyReviewText()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "",
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.NotNull(result);
            Assert.Equal("", result.ReviewText);
        }

        [Fact]
        public async Task CreateReview_ShouldHandleLongReviewText()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var longText = new string('A', 5000);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = longText,
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.NotNull(result);
            Assert.Equal(longText, result.ReviewText);
        }

        [Fact]
        public async Task CreateReview_ShouldHandleSpecialCharactersInReviewText()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var specialText = "Ótimo trabalho! @#$%^&*() ção çã é";

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = specialText,
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.NotNull(result);
            Assert.Equal(specialText, result.ReviewText);
        }

        [Fact]
        public async Task CreateReview_ShouldGenerateId()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test",
                Rating = 5,
                ProposalId = 99
            };

            var result = await service.CreateReview(dto);

            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateReview_ShouldGenerateUniqueIds_ForMultipleReviews()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto1 = new ReviewCreate
            {
                ReviewerId = 1,
                ReceiverId = 2,
                ReviewText = "Test 1",
                Rating = 5,
                ProposalId = 99
            };

            var dto2 = new ReviewCreate
            {
                ReviewerId = 2,
                ReceiverId = 1,
                ReviewText = "Test 2",
                Rating = 4,
                ProposalId = 100
            };

            var result1 = await service.CreateReview(dto1);
            var result2 = await service.CreateReview(dto2);

            Assert.NotEqual(result1.Id, result2.Id);
        }
    }
}
