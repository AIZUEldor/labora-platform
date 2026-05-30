namespace Labora.Application.DTOs.Reviews;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerFirstName { get; set; } = string.Empty;
    public string ReviewerLastName { get; set; } = string.Empty;
    public Guid RevieweeId { get; set; }
    public Guid JobApplicationId { get; set; }
    public int OverallRating { get; set; }

    // Worker → Employer kategoriyalari
    public int? PaymentRating { get; set; }
    public int? EmployerCommunicationRating { get; set; }

    // Employer → Worker kategoriyalari
    public int? ExperienceRating { get; set; }
    public int? WorkerCommunicationRating { get; set; }
    public int? WorkQualityRating { get; set; }
    public int? PunctualityRating { get; set; }
    public int? ResponsibilityRating { get; set; }

    // Ha/Yo'q
    public bool WouldWorkAgain { get; set; }

    // Izoh
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}