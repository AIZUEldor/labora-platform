using Labora.Application.DTOs.Transactions;

namespace Labora.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponseDto> CreateAsync(TransactionRequestDto request, Guid userId);
    Task<IEnumerable<TransactionResponseDto>> GetByUserIdAsync(Guid userId);
    Task<TransactionResponseDto> GetByIdAsync(Guid id);
}