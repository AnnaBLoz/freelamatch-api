using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

public interface IReviewsService
{
    Task<List<Reviews>> GetReviews(int userId);
    Task<Reviews> CreateReview(ReviewCreate reviewCreate);
}
