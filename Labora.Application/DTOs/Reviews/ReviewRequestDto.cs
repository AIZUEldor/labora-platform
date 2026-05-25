namespace Labora.Application.DTOs.Reviews;

public class ReviewRequestDto
{
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
}