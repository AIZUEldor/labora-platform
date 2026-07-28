using Labora.Application.DTOs.Properties;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IPropertyListingService
{
    Task<PropertyListingResponseDto> CreateAsync(PropertyListingRequestDto request, List<IFormFile> images, Guid ownerId);
    Task<PropertyListingResponseDto> UpdateAsync(Guid id, PropertyListingRequestDto request, Guid ownerId);
    Task DeleteAsync(Guid id, Guid ownerId);
    Task<PropertyListingResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyListingResponseDto>> GetAllPublishedAsync();
    Task<IEnumerable<PropertyListingResponseDto>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<PropertyMarkerDto>> GetPublishedMarkersAsync();

    Task<PropertyImageDto> AddImageAsync(Guid propertyId, Guid ownerId, IFormFile file);
    Task DeleteImageAsync(Guid propertyId, Guid ownerId, Guid imageId);
    Task<IEnumerable<PropertyImageDto>> ReorderImagesAsync(Guid propertyId, Guid ownerId, IReadOnlyList<Guid> orderedImageIds);
}
