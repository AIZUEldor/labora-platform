using Labora.Application.DTOs.Properties;

namespace Labora.Application.Interfaces;

public interface IPropertyListingService
{
    Task<PropertyListingResponseDto> CreateAsync(PropertyListingRequestDto request, Guid ownerId);
    Task<PropertyListingResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyListingResponseDto>> GetAllPublishedAsync();
    Task<IEnumerable<PropertyMarkerDto>> GetPublishedMarkersAsync();
}
