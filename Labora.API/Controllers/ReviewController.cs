using Labora.Application.DTOs.Reviews;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewRequestDto request)
    {
        Guid reviewerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ReviewResponseDto response = await _reviewService.CreateAsync(request, reviewerId);
        return Ok(response);
    }

    [HttpGet("user/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewsByUserId(Guid userId)
    {
        IEnumerable<ReviewResponseDto> reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
        return Ok(reviews);
    }

    [HttpGet("user/{userId:guid}/average")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAverageRating(Guid userId)
    {
        double averageRating = await _reviewService.GetAverageRatingAsync(userId);
        return Ok(new { userId, averageRating });
    }
}