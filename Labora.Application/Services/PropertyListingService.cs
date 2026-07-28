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

    // Mirrors JobService.UpdateAsync: images-inclusive fetch (now tracked - see the repository
    // comment) survives _mapper.Map(request, propertyListing) untouched, since
    // PropertyListingRequestDto has no Images property; OwnerId/Status are also protected by the
    // same CreateMap<PropertyListingRequestDto, PropertyListing>() Ignore()s used by CreateAsync.
    // Validation lives here rather than in the controller, matching this service's own
    // CreateAsync convention (not JobController's, which validates before calling the service).
    public async Task<PropertyListingResponseDto> UpdateAsync(Guid id, PropertyListingRequestDto request, Guid ownerId)
    {
        ValidationResult validationResult = await _propertyListingValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdWithImagesAsync(id);

        if (propertyListing is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ko'chmas mulk e'loni topilmadi.");

        if (propertyListing.OwnerId != ownerId)
            throw new InvalidOperationException("Siz bu e'lonni tahrirlash huquqiga ega emassiz.");

        _mapper.Map(request, propertyListing);

        PropertyListing updated = await _propertyListingRepository.UpdateAsync(propertyListing);
        return _mapper.Map<PropertyListingResponseDto>(updated);
    }

    // Mirrors JobService.DeleteAsync exactly: plain (non-images) fetch for the ownership check,
    // then the inherited GenericRepository.DeleteAsync soft-delete (sets IsDeleted, never touches
    // PropertyImage rows). Every existing read query (GetAllPublishedAsync, GetByOwnerIdAsync,
    // GetByIdWithImagesAsync, GetPublishedMarkersAsync) already filters !IsDeleted, so this alone
    // removes the listing from the published list, map markers and public/owner detail - no query
    // changes needed. PropertyImage rows are left untouched (same convention as JobImage on Job
    // delete), preserving them and the listing row itself as history, not hard-removed.
    public async Task DeleteAsync(Guid id, Guid ownerId)
    {
        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdAsync(id);

        if (propertyListing is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ko'chmas mulk e'loni topilmadi.");

        if (propertyListing.OwnerId != ownerId)
            throw new InvalidOperationException("Siz bu e'lonni o'chirish huquqiga ega emassiz.");

        await _propertyListingRepository.DeleteAsync(id);
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

    public async Task<IEnumerable<PropertyListingResponseDto>> GetByOwnerIdAsync(Guid ownerId)
    {
        IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)> listings =
            await _propertyListingRepository.GetByOwnerIdAsync(ownerId);

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

    // Mirrors JobService.AddImageAsync: file-first (validated, saved to disk), then the DB row - if
    // the DB write fails the already-written file is deleted so storage never ends up ahead of the
    // database. New images append after the current highest SortOrder in the (already-ordered)
    // Images collection; SortOrder is left as-is for the rest of the gallery (no renormalization),
    // matching CreateAsync's "SortOrder preserves upload order" convention.
    public async Task<PropertyImageDto> AddImageAsync(Guid propertyId, Guid ownerId, IFormFile file)
    {
        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdWithImagesAsync(propertyId);
        if (propertyListing is null)
            throw new InvalidOperationException($"Id={propertyId} bo'lgan ko'chmas mulk e'loni topilmadi.");

        if (propertyListing.OwnerId != ownerId)
            throw new InvalidOperationException("Siz bu e'longa rasm qo'sha olmaysiz.");

        if (propertyListing.Images.Count >= MaxPropertyImages)
            throw new ArgumentException($"Maksimal {MaxPropertyImages} ta rasm yuklash mumkin.");

        ImageUploadValidator.Validate(file);

        string imageUrl = await _fileStorageService.SaveAsync(file, PropertyImagesSubFolder);

        int nextSortOrder = propertyListing.Images.Count == 0
            ? 0
            : propertyListing.Images.Max(i => i.SortOrder) + 1;

        try
        {
            PropertyImage image = await _propertyListingRepository.AddImageAsync(propertyId, imageUrl, imageUrl, nextSortOrder);
            return new PropertyImageDto
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                SortOrder = image.SortOrder
            };
        }
        catch
        {
            _fileStorageService.Delete(imageUrl);
            throw;
        }
    }

    // Mirrors JobService.DeleteImageAsync's DB-first-then-file cleanup order: the physical file is
    // only removed once the (soft) delete is confirmed, and a missing/inaccessible file at that point
    // must never turn an already-successful delete into a failed request (IFileStorageService.Delete
    // never throws for that). MinPropertyImages is enforced here the same way CreateAsync enforces it
    // on the way in, so a listing can never be left with zero images through this endpoint.
    public async Task DeleteImageAsync(Guid propertyId, Guid ownerId, Guid imageId)
    {
        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdWithImagesAsync(propertyId);
        if (propertyListing is null)
            throw new InvalidOperationException($"Id={propertyId} bo'lgan ko'chmas mulk e'loni topilmadi.");

        if (propertyListing.OwnerId != ownerId)
            throw new InvalidOperationException("Siz bu e'londan rasm o'chira olmaysiz.");

        PropertyImage? image = propertyListing.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new InvalidOperationException("Rasm ushbu e'longa tegishli emas.");

        if (propertyListing.Images.Count <= MinPropertyImages)
            throw new ArgumentException($"Kamida {MinPropertyImages} ta rasm bo'lishi shart.");

        await _propertyListingRepository.DeleteImageAsync(imageId);
        _fileStorageService.Delete(image.ImageUrl);
    }

    // Reorder request must name every currently-active image exactly once - not a subset, no
    // duplicates, no foreign ids - since a partial list would silently leave the untouched images'
    // SortOrder stale relative to the ones just renumbered. SortOrder is normalized to a dense
    // 0..n-1 sequence in the caller's requested order; index 0 is the cover image (mirrors
    // CreateAsync's "0 is the cover image" convention, now made reassignable).
    public async Task<IEnumerable<PropertyImageDto>> ReorderImagesAsync(Guid propertyId, Guid ownerId, IReadOnlyList<Guid> orderedImageIds)
    {
        PropertyListing? propertyListing = await _propertyListingRepository.GetByIdWithImagesAsync(propertyId);
        if (propertyListing is null)
            throw new InvalidOperationException($"Id={propertyId} bo'lgan ko'chmas mulk e'loni topilmadi.");

        if (propertyListing.OwnerId != ownerId)
            throw new InvalidOperationException("Siz bu e'londagi rasmlar tartibini o'zgartira olmaysiz.");

        if (orderedImageIds is null || orderedImageIds.Count == 0)
            throw new ArgumentException("Rasmlar tartibi bo'sh bo'lishi mumkin emas.");

        if (orderedImageIds.Distinct().Count() != orderedImageIds.Count)
            throw new ArgumentException("Rasm ID'lari takrorlanmasligi kerak.");

        HashSet<Guid> activeImageIds = propertyListing.Images.Select(i => i.Id).ToHashSet();
        if (!activeImageIds.SetEquals(orderedImageIds))
            throw new ArgumentException("Rasmlar ro'yxati e'lonning joriy faol rasmlariga to'liq mos kelishi kerak.");

        List<(Guid ImageId, int SortOrder)> normalized = orderedImageIds
            .Select((imageId, index) => (ImageId: imageId, SortOrder: index))
            .ToList();

        await _propertyListingRepository.ReorderImagesAsync(normalized);

        return normalized.Select(n => new PropertyImageDto
        {
            Id = n.ImageId,
            ImageUrl = propertyListing.Images.First(i => i.Id == n.ImageId).ImageUrl,
            SortOrder = n.SortOrder
        });
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
