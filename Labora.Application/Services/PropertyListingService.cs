using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Labora.Application.DTOs.Properties;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class PropertyListingService : IPropertyListingService
{
    private const int MinPropertyImages = 1;
    private const int MaxPropertyImages = 10;
    private const string PropertyImagesSubFolder = "property-images";

    private readonly IPropertyListingRepository _propertyListingRepository;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IValidator<PropertyListingRequestDto> _propertyListingValidator;

    public PropertyListingService(
        IPropertyListingRepository propertyListingRepository,
        IMapper mapper,
        IFileStorageService fileStorageService,
        IValidator<PropertyListingRequestDto> propertyListingValidator)
    {
        _propertyListingRepository = propertyListingRepository;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _propertyListingValidator = propertyListingValidator;
    }

    public async Task<PropertyListingResponseDto> CreateAsync(PropertyListingRequestDto request, List<IFormFile> images, Guid ownerId)
    {
        ValidationResult validationResult = await _propertyListingValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        if (images is null || images.Count < MinPropertyImages || images.Count > MaxPropertyImages)
            throw new ArgumentException($"Kamida {MinPropertyImages} va ko'pi bilan {MaxPropertyImages} ta rasm yuklash mumkin.");

        // Tracked so a failed upload or DB save can delete every file already written this request.
        List<string> uploadedUrls = new();
        try
        {
            List<PropertyImage> propertyImages = new();
            for (int i = 0; i < images.Count; i++)
            {
                IFormFile file = images[i];
                ImageUploadValidator.Validate(file);
                string imageUrl = await _fileStorageService.SaveAsync(file, PropertyImagesSubFolder);
                uploadedUrls.Add(imageUrl);

                // SortOrder preserves upload order; 0 is the cover image.
                propertyImages.Add(new PropertyImage
                {
                    ImageUrl = imageUrl,
                    StorageKey = imageUrl,
                    SortOrder = i
                });
            }

            PropertyListing propertyListing = _mapper.Map<PropertyListing>(request);
            propertyListing.OwnerId = ownerId;
            propertyListing.Status = PropertyListingStatus.Published;
            propertyListing.Images = propertyImages;

            PropertyListing created = await _propertyListingRepository.AddAsync(propertyListing);

            return _mapper.Map<PropertyListingResponseDto>(created);
        }
        catch
        {
            foreach (string url in uploadedUrls)
            {
                _fileStorageService.Delete(url);
            }
            throw;
        }
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
