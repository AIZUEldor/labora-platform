using Labora.Application.DTOs.Properties;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IPropertyListingService
{
    Task<PropertyListingResponseDto> CreateAsync(PropertyListingRequestDto request, List<IFormFile> images, Guid ownerId);
    Task<PropertyListingResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyListingResponseDto>> GetAllPublishedAsync();
    Task<IEnumerable<PropertyListingResponseDto>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<PropertyMarkerDto>> GetPublishedMarkersAsync();
}
