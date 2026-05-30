using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class UserPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? PreferredCategoryId { get; set; }
    public JobType? PreferredJobType { get; set; }
    public double? MaxDistanceKm { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Category? PreferredCategory { get; set; }
}