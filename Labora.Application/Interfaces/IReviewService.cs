using Labora.Application.DTOs.Reviews;

namespace Labora.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewResponseDto> CreateAsync(ReviewRequestDto request, Guid reviewerId);
    Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(Guid userId);
    Task<double> GetAverageRatingAsync(Guid userId);
}