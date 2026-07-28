using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class PropertyListingRepository : GenericRepository<PropertyListing>, IPropertyListingRepository
{
    private readonly LaboaDbContext _context;

    public PropertyListingRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    // Separate from the inherited GenericRepository.GetByIdAsync (which never loads Images) so the
    // detail response's CoverImageUrl/gallery - derived from Images in MappingProfile - reflects
    // real persisted PropertyImage rows, same convention as JobRepository.GetByIdWithImagesAsync.
    // Tracked (no AsNoTracking) - PropertyListingService.UpdateAsync reuses this fetch and mutates
    // the returned graph in place before saving, exactly like JobRepository.GetByIdWithImagesAsync.
    public async Task<PropertyListing?> GetByIdWithImagesAsync(Guid id)
    {
        return await _context.PropertyListings
            .Include(p => p.Images
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id))
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetAllPublishedAsync()
    {
        // Same single-query cover-image projection as JobRepository.GetJobsByEmployerIdAsync - no
        // Include(p => p.Images) here, so the full gallery is never fetched for this list endpoint.
        var rows = await _context.PropertyListings
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == PropertyListingStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                PropertyListing = p,
                CoverImageUrl = p.Images
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows.Select(r => (r.PropertyListing, r.CoverImageUrl));
    }

    public async Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetByOwnerIdAsync(Guid ownerId)
    {
        // Same cover-image projection as GetAllPublishedAsync, but no Status filter - the owner's
        // own list includes both Published and Archived listings.
        var rows = await _context.PropertyListings
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                PropertyListing = p,
                CoverImageUrl = p.Images
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows.Select(r => (r.PropertyListing, r.CoverImageUrl));
    }

    public async Task<IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)>> GetPublishedMarkersAsync()
    {
        var rows = await _context.PropertyListings
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == PropertyListingStatus.Published)
            .Select(p => new
            {
                p.Id,
                p.Latitude,
                p.Longitude,
                p.Price,
                p.PropertyType
            })
            .ToListAsync();

        return rows.Select(r => (r.Id, r.Latitude, r.Longitude, r.Price, r.PropertyType));
    }

    // Mirrors JobRepository.AddImageAsync; StorageKey/SortOrder are PropertyImage-specific columns
    // JobImage doesn't have.
    public async Task<PropertyImage> AddImageAsync(Guid propertyListingId, string imageUrl, string storageKey, int sortOrder)
    {
        PropertyImage image = new()
        {
            ImageUrl = imageUrl,
            StorageKey = storageKey,
            SortOrder = sortOrder,
            PropertyListingId = propertyListingId
        };
        await _context.PropertyImages.AddAsync(image);
        await _context.SaveChangesAsync();
        return image;
    }

    // Soft delete, unlike JobRepository.DeleteImageAsync's hard delete - PropertyImage rows follow
    // this codebase's IsDeleted convention, and every existing read query (GetByIdWithImagesAsync,
    // the cover-image projections above) already filters !IsDeleted, so this alone removes the image
    // from every future response without any query changes.
    public async Task DeleteImageAsync(Guid imageId)
    {
        PropertyImage? image = await _context.PropertyImages
            .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);
        if (image != null)
        {
            image.IsDeleted = true;
            image.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderImagesAsync(IReadOnlyList<(Guid ImageId, int SortOrder)> orderedImages)
    {
        List<Guid> imageIds = orderedImages.Select(o => o.ImageId).ToList();
        List<PropertyImage> images = await _context.PropertyImages
            .Where(i => imageIds.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync();

        foreach ((Guid imageId, int sortOrder) in orderedImages)
        {
            PropertyImage? image = images.FirstOrDefault(i => i.Id == imageId);
            if (image is not null)
            {
                image.SortOrder = sortOrder;
                image.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }
}
