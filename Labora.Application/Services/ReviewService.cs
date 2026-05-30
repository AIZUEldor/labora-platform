using AutoMapper;
using Labora.Application.DTOs.Reviews;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ReviewService(
        IReviewRepository reviewRepository,
        IJobApplicationRepository jobApplicationRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ReviewResponseDto> CreateAsync(ReviewRequestDto request, Guid reviewerId)
    {
        JobApplication? jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        if (jobApplication is null)
            throw new InvalidOperationException($"Id={request.JobApplicationId} bo'lgan ariza topilmadi.");

        if (jobApplication.Status != ApplicationStatus.Completed)
            throw new InvalidOperationException("Faqat yakunlangan ishlarni baholash mumkin.");

        bool hasReviewed = await _reviewRepository.HasReviewedAsync(reviewerId, request.JobApplicationId);

        if (hasReviewed)
            throw new InvalidOperationException("Siz bu ishni allaqachon baholagansiz.");

        User? reviewer = await _userRepository.GetByIdAsync(reviewerId);

        if (reviewer is null)
            throw new InvalidOperationException("Foydalanuvchi topilmadi.");

        Guid revieweeId;

        if (reviewer.Role == UserRole.Worker)
            revieweeId = jobApplication.Job.EmployerId;
        else
            revieweeId = jobApplication.WorkerId;

        Review review = new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            JobApplicationId = request.JobApplicationId,
            OverallRating = request.OverallRating,
            PaymentRating = request.PaymentRating,
            EmployerCommunicationRating = request.EmployerCommunicationRating,
            ExperienceRating = request.ExperienceRating,
            WorkerCommunicationRating = request.WorkerCommunicationRating,
            WorkQualityRating = request.WorkQualityRating,
            PunctualityRating = request.PunctualityRating,
            ResponsibilityRating = request.ResponsibilityRating,
            WouldWorkAgain = request.WouldWorkAgain,
            Comment = request.Comment
        };

        Review createdReview = await _reviewRepository.AddAsync(review);

        ReviewResponseDto response = _mapper.Map<ReviewResponseDto>(createdReview);
        response.ReviewerFirstName = reviewer.FirstName;
        response.ReviewerLastName = reviewer.LastName;

        return response;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(Guid userId)
    {
        IEnumerable<Review> reviews = await _reviewRepository.GetReviewsByRevieweeIdAsync(userId);

        return reviews.Select(review => new ReviewResponseDto
        {
            Id = review.Id,
            ReviewerId = review.ReviewerId,
            ReviewerFirstName = review.Reviewer.FirstName,
            ReviewerLastName = review.Reviewer.LastName,
            RevieweeId = review.RevieweeId,
            JobApplicationId = review.JobApplicationId,
            OverallRating = review.OverallRating,
            PaymentRating = review.PaymentRating,
            EmployerCommunicationRating = review.EmployerCommunicationRating,
            ExperienceRating = review.ExperienceRating,
            WorkerCommunicationRating = review.WorkerCommunicationRating,
            WorkQualityRating = review.WorkQualityRating,
            PunctualityRating = review.PunctualityRating,
            ResponsibilityRating = review.ResponsibilityRating,
            WouldWorkAgain = review.WouldWorkAgain,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        });
    }

    public async Task<double> GetAverageRatingAsync(Guid userId)
    {
        return await _reviewRepository.GetAverageRatingAsync(userId);
    }
}