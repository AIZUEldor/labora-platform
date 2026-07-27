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
    public async Task<IActionResult> Create([FromBody] PropertyListingRequestDto request)
    {
        Guid ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PropertyListingResponseDto result = await _propertyListingService.CreateAsync(request, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<PropertyListingResponseDto> result = await _propertyListingService.GetAllPublishedAsync();
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
