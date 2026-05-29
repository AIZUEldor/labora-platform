using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Notifications;

public class UserPreferenceRequestDto
{
    public List<Guid> PreferredCategoryIds { get; set; } = new();
    public JobType? PreferredJobType { get; set; }
    public double? MaxDistanceKm { get; set; }
}