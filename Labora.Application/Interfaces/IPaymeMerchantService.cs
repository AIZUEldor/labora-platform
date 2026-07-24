using Labora.Application.DTOs.Payments.Payme;

namespace Labora.Application.Interfaces;

public interface IPaymeMerchantService
{
    /// <summary>
    /// Protocol adapter entry point: validates Basic Auth, parses the JSON-RPC envelope, dispatches
    /// to the matching method below, and translates any failure into a PaymeRpcResponse error - never
    /// throws, so the controller can always respond with HTTP 200 as Payme's protocol requires.
    /// </summary>
    Task<PaymeRpcResponse<object?>> HandleRequestAsync(string rawRequestBody, string? authorizationHeader);

    Task<CheckPerformTransactionResponseDto> CheckPerformTransactionAsync(CheckPerformTransactionRequestDto request);
    Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto request);
    Task<PerformTransactionResponseDto> PerformTransactionAsync(PerformTransactionRequestDto request);
    Task<CancelTransactionResponseDto> CancelTransactionAsync(CancelTransactionRequestDto request);
    Task<CheckTransactionResponseDto> CheckTransactionAsync(CheckTransactionRequestDto request);
    Task<GetStatementResponseDto> GetStatementAsync(GetStatementRequestDto request);
}
