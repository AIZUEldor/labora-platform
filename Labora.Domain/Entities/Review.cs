using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class Review : BaseEntity
{
    // Kim baholadi va kim baholandi
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public Guid JobApplicationId { get; set; }

    // Umumiy reyting (1-5)
    public int OverallRating { get; set; }

    // Worker → Employer baholash kategoriyalari
    public int? PaymentRating { get; set; }          // Pulni kelishilgan holda berishi
    public int? EmployerCommunicationRating { get; set; }  // Muomalasi
    public int? WorkConditionRating { get; set; }  // Ish sharoiti

    // Employer → Worker baholash kategoriyalari
    public int? ExperienceRating { get; set; }       // Ish tajribasi
    public int? WorkerCommunicationRating { get; set; }    // Muomalasi
    public int? WorkQualityRating { get; set; }      // Ish sifati
    public int? PunctualityRating { get; set; }      // O'z vaqtida bajarganligi
    public int? ResponsibilityRating { get; set; }   // Ishiga mas'uliyati

    // Ha/Yo'q savollar
    public bool WouldWorkAgain { get; set; }         // Yana ishlagan bo'larmidim / Yana ishga olaman

    // Izoh
    public string? Comment { get; set; }

    // Navigation properties
    public User Reviewer { get; set; } = null!;
    public User Reviewee { get; set; } = null!;
    public JobApplication JobApplication { get; set; } = null!;
}