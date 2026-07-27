using Labora.API.Models;
using Labora.Application.DTOs.Properties;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyListingController : ControllerBase
{
    private readonly IPropertyListingService _propertyListingService;

    public PropertyListingController(IPropertyListingService propertyListingService)
    {
        _propertyListingService = propertyListingService;
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] PropertyListingCreateForm form)
    {
        PropertyListingRequestDto request = new()
        {
            Title = form.Title,
            Description = form.Description,
            PropertyType = form.PropertyType,
            RoomCount = form.RoomCount,
            AreaSquareMeters = form.AreaSquareMeters,
            FloorNumber = form.FloorNumber,
            TotalFloors = form.TotalFloors,
            RenovationStatus = form.RenovationStatus,
            Price = form.Price,
            RentalPeriod = form.RentalPeriod,
            Address = form.Address,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            ContactPhoneNumber = form.ContactPhoneNumber
        };

        Guid ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PropertyListingResponseDto result = await _propertyListingService.CreateAsync(request, form.Images, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<PropertyListingResponseDto> result = await _propertyListingService.GetAllPublishedAsync();
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMy()
    {
        Guid ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<PropertyListingResponseDto> result = await _propertyListingService.GetByOwnerIdAsync(ownerId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        PropertyListingResponseDto result = await _propertyListingService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("markers")]
    public async Task<IActionResult> GetPublishedMarkers()
    {
        IEnumerable<PropertyMarkerDto> result = await _propertyListingService.GetPublishedMarkersAsync();
        return Ok(result);
    }
}
