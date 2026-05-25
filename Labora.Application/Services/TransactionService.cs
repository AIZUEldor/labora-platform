using AutoMapper;
using Labora.Application.DTOs.Transactions;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<TransactionResponseDto> CreateAsync(TransactionRequestDto request, Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        if (request.Type == "withdrawal" && user.Balance < request.Amount)
            throw new InvalidOperationException("Balansingiz yetarli emas.");

        if (request.Type == "deposit")
            user.Balance += request.Amount;
        else if (request.Type == "withdrawal")
            user.Balance -= request.Amount;
        else
            throw new InvalidOperationException("Noto'g'ri tranzaksiya turi. 'deposit' yoki 'withdrawal' bo'lishi kerak.");

        await _userRepository.UpdateAsync(user);

        Transaction transaction = new Transaction
        {
            Amount = request.Amount,
            Type = request.Type,
            Description = request.Description,
            UserId = userId
        };

        Transaction createdTransaction = await _transactionRepository.AddAsync(transaction);
        return _mapper.Map<TransactionResponseDto>(createdTransaction);
    }

    public async Task<IEnumerable<TransactionResponseDto>> GetByUserIdAsync(Guid userId)
    {
        IEnumerable<Transaction> transactions = await _transactionRepository.GetTransactionsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<TransactionResponseDto>>(transactions);
    }

    public async Task<TransactionResponseDto> GetByIdAsync(Guid id)
    {
        Transaction? transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction is null)
            throw new InvalidOperationException($"Id={id} bo'lgan tranzaksiya topilmadi.");

        return _mapper.Map<TransactionResponseDto>(transaction);
    }
}