using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Foreign key
    public Guid PropertyListingId { get; set; }

    // Navigation property
    public PropertyListing PropertyListing { get; set; } = null!;
}
