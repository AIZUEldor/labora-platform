using Labora.Application.DTOs.Payments;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("top-up")]
    public async Task<IActionResult> TopUp([FromBody] CreateTopUpRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PaymentOrderResponseDto response = await _paymentService.CreateTopUpAsync(request, userId);
        return Ok(response);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PaymentOrderResponseDto response = await _paymentService.GetByIdAsync(orderId, userId);
        return Ok(response);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<PaymentOrderResponseDto> response = await _paymentService.GetMyPaymentOrdersAsync(userId);
        return Ok(response);
    }
}
