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
    }
}
