using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Applications;

public class ApplicationResponseDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid WorkerId { get; set; }
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}