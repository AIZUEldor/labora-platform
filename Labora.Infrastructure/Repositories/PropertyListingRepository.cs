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
    // AsNoTracking is safe here: unlike Job, there is no update/delete flow yet that reuses this
    // fetch for a save.
    public async Task<PropertyListing?> GetByIdWithImagesAsync(Guid id)
    {
        return await _context.PropertyListings
            .AsNoTracking()
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
}
