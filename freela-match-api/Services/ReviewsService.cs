using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;

public class ReviewsService : IReviewsService
{
    private readonly AppDbContext _context;

    public ReviewsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reviews>> GetReviews(int userId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Receiver)
            .Where(r => r.ReceiverId == userId || r.ReviewerId == userId)
            .ToListAsync();
    }

    public async Task<Reviews> CreateReview(ReviewCreate reviewCreated)
    {
        var review = new Reviews
        {
            ReviewerId = reviewCreated.ReviewerId,
            ReceiverId = reviewCreated.ReceiverId,
            ReviewText = reviewCreated.ReviewText,
            Rating = reviewCreated.Rating,
            ProposalId = reviewCreated.ProposalId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(review);
        await _context.SaveChangesAsync();

        // Atualiza o status do Candidate aceito para Reviewed
        var candidate = await _context.Candidate
            .Where(c => c.UserId == reviewCreated.ReceiverId
                     && c.ProposalId == reviewCreated.ProposalId
                     && c.Status == ProposalStatus.Accepted)
            .FirstOrDefaultAsync();

        if (candidate != null)
        {
            candidate.Status = ProposalStatus.Reviewed;
            _context.Candidate.Update(candidate);
            await _context.SaveChangesAsync();
        }

        return review;
    }
}
