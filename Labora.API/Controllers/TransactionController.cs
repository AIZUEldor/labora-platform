using Labora.Application.DTOs.Transactions;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TransactionRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        TransactionResponseDto response = await _transactionService.CreateAsync(request, userId);
        return Ok(response);
    }

    [HttpGet("my-transactions")]
    public async Task<IActionResult> GetMyTransactions()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<TransactionResponseDto> transactions = await _transactionService.GetByUserIdAsync(userId);
        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        TransactionResponseDto transaction = await _transactionService.GetByIdAsync(id);
        return Ok(transaction);
    }
}