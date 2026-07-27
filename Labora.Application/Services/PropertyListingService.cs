using AutoMapper;
using Labora.Application.DTOs.Properties;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class PropertyListingService : IPropertyListingService
{
    private readonly IPropertyListingRepository _propertyListingRepository;
    private readonly IMapper _mapper;

    public PropertyListingService(IPropertyListingRepository propertyListingRepository, IMapper mapper)
    {
        _propertyListingRepository = propertyListingRepository;
        _mapper = mapper;
    }

    public async Task<PropertyListingResponseDto> CreateAsync(PropertyListingRequestDto request, Guid ownerId)
    {
        PropertyListing propertyListing = _mapper.Map<PropertyListing>(request);
        propertyListing.OwnerId = ownerId;
        propertyListing.Status = PropertyListingStatus.Published;

        PropertyListing created = await _propertyListingRepository.AddAsync(propertyListing);

        return _mapper.Map<PropertyListingResponseDto>(created);
    }

    public async Task<PropertyListingResponseDto> GetByIdAsync(Guid id)
    {
        // Images-inclusive fetch (see PropertyListingRepository.GetByIdWithImagesAsync), so
        // CoverImageUrl reflects real persisted PropertyImage rows instead of always evaluating
        // to null.
        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdWithImagesAsync(id);

        if (propertyListing is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ko'chmas mulk e'loni topilmadi.");

        return _mapper.Map<PropertyListingResponseDto>(propertyListing);
    }

    public async Task<IEnumerable<PropertyListingResponseDto>> GetAllPublishedAsync()
    {
        IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)> listings =
            await _propertyListingRepository.GetAllPublishedAsync();

        return listings.Select(MapToListItem);
    }

    // Single conversion point for the list endpoint: maps scalar fields via the normal AutoMapper
    // profile, then overwrites CoverImageUrl with the value the repository already computed cheaply
    // (the AutoMapper-derived one would otherwise be null here, since Images was never loaded for
    // this query shape - see the MappingProfile comment). Mirrors JobService.MapToListItem.
    private PropertyListingResponseDto MapToListItem((PropertyListing PropertyListing, string? CoverImageUrl) item)
    {
        PropertyListingResponseDto dto = _mapper.Map<PropertyListingResponseDto>(item.PropertyListing);
        dto.CoverImageUrl = item.CoverImageUrl;
        return dto;
    }

    public async Task<IEnumerable<PropertyMarkerDto>> GetPublishedMarkersAsync()
    {
        IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)> markers =
            await _propertyListingRepository.GetPublishedMarkersAsync();

        return markers.Select(m => new PropertyMarkerDto
        {
            Id = m.Id,
            Latitude = m.Latitude,
            Longitude = m.Longitude,
            Price = m.Price,
            PropertyType = m.PropertyType
        });
    }
}
