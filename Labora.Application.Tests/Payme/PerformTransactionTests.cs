using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Options;
using Labora.Application.Services;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;

namespace Labora.Application.Tests.Payme;

public class PerformTransactionTests
{
    private static PaymentOrder CreateOrder(decimal amountSoum = 10_000m, PaymentOrderStatus status = PaymentOrderStatus.Pending)
    {
        return new PaymentOrder
        {
            Id = Guid.NewGuid(),
            Amount = amountSoum,
            Provider = PaymentProvider.Payme,
            Status = status,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    private static User CreateUser(Guid id, decimal startingBalance = 0m)
    {
        return new User
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = $"+998{Random.Shared.Next(100000000, 999999999)}",
            Balance = startingBalance
        };
    }

    private static PaymeTransaction CreateCreatedTransaction(PaymentOrder order, string paymeTransactionId = "payme-tx-1")
    {
        return new PaymeTransaction
        {
            Id = Guid.NewGuid(),
            PaymentOrderId = order.Id,
            PaymeTransactionId = paymeTransactionId,
            AccountReference = order.Id.ToString(),
            PaymeTransactionTime = 1,
            RequestedAmountTiyin = (long)(order.Amount * 100m),
            MerchantCreateTime = 1,
            InternalStatus = PaymeTransactionInternalStatus.Created,
            PaymeStateCode = 1
        };
    }

    private static (PaymeMerchantService Service, FakePaymentOrderRepository OrderRepository, FakePaymeTransactionRepository TransactionRepository, FakeUserRepository UserRepository) CreateService(
        PaymentOrder[] orders, User[] users, PaymeTransaction[] transactions)
    {
        FakePaymentOrderRepository orderRepository = new(orders);
        FakeUserRepository userRepository = new(users);
        FakePaymeTransactionRepository transactionRepository = new();
        foreach (PaymeTransaction transaction in transactions)
        {
            transactionRepository.SeedDirectly(transaction);
        }

        PaymeAuthenticator authenticator = new(Microsoft.Extensions.Options.Options.Create(new PaymeMerchantOptions
        {
            MerchantId = "merchant-1",
            Login = "Paycom",
            Password = "secret"
        }));

        PaymeMerchantService service = new(authenticator, orderRepository, transactionRepository, userRepository, new PassThroughUnitOfWork());
        return (service, orderRepository, transactionRepository, userRepository);
    }

    private static PerformTransactionRequestDto Request(string id) => new() { Id = id };

    [Fact]
    public async Task PerformTransactionAsync_FirstCall_TransitionsCreatedToPerformed()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        User user = CreateUser(order.UserId, startingBalance: 500m);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) = CreateService([order], [user], [transaction]);

        await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(PaymeTransactionInternalStatus.Performed, stored!.InternalStatus);
        Assert.NotNull(stored.MerchantPerformTime);
    }

    [Fact]
    public async Task PerformTransactionAsync_FirstCall_CreditsUserBalanceByOrderAmount()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        User user = CreateUser(order.UserId, startingBalance: 500m);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) = CreateService([order], [user], [transaction]);

        await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        User? updatedUser = userRepository.GetRaw(order.UserId);
        Assert.Equal(500m + 10_000m, updatedUser!.Balance);
    }

    [Fact]
    public async Task PerformTransactionAsync_FirstCall_TransitionsOrderPendingToPaidAndSetsPaidAt()
    {
        PaymentOrder order = CreateOrder();
        User user = CreateUser(order.UserId);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, _, _) = CreateService([order], [user], [transaction]);

        await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        PaymentOrder? updatedOrder = orderRepository.GetRaw(order.Id);
        Assert.Equal(PaymentOrderStatus.Paid, updatedOrder!.Status);
        Assert.NotNull(updatedOrder.PaidAt);
    }

    [Fact]
    public async Task PerformTransactionAsync_FirstCall_ResponseMatchesStoredTransaction()
    {
        PaymentOrder order = CreateOrder();
        User user = CreateUser(order.UserId);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) = CreateService([order], [user], [transaction]);

        PerformTransactionResponseDto response = await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(stored!.Id.ToString(), response.Transaction);
        Assert.NotEqual(order.Id.ToString(), response.Transaction);
        Assert.Equal(stored.MerchantPerformTime, response.PerformTime);
        Assert.Equal(2, response.State);
    }

    [Fact]
    public async Task PerformTransactionAsync_RepeatedCall_ReturnsIdempotentSuccessWithoutCreditingTwice()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        User user = CreateUser(order.UserId, startingBalance: 0m);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) = CreateService([order], [user], [transaction]);

        PerformTransactionResponseDto first = await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));
        PerformTransactionResponseDto second = await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(first.Transaction, second.Transaction);
        Assert.Equal(first.PerformTime, second.PerformTime);
        Assert.Equal(first.State, second.State);
        Assert.Equal(10_000m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(1, userRepository.UpdateAsyncCallCount);
    }

    [Fact]
    public async Task PerformTransactionAsync_TransactionNotFound_ThrowsTransactionNotFound()
    {
        (PaymeMerchantService service, _, _, _) = CreateService([], [], []);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.PerformTransactionAsync(Request("unknown-id")));

        Assert.Equal(PaymeErrorCodes.TransactionNotFound, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task PerformTransactionAsync_CancelledTransaction_ThrowsOperationNotAllowed()
    {
        PaymentOrder order = CreateOrder();
        User user = CreateUser(order.UserId);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        transaction.InternalStatus = PaymeTransactionInternalStatus.Cancelled;
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) = CreateService([order], [user], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.PerformTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
    }

    [Theory]
    [InlineData(PaymentOrderStatus.Paid)]
    [InlineData(PaymentOrderStatus.Cancelled)]
    [InlineData(PaymentOrderStatus.Failed)]
    [InlineData(PaymentOrderStatus.Expired)]
    public async Task PerformTransactionAsync_LinkedOrderNotPending_ThrowsOperationNotAllowed(PaymentOrderStatus orderStatus)
    {
        PaymentOrder order = CreateOrder(status: orderStatus);
        User user = CreateUser(order.UserId);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) = CreateService([order], [user], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.PerformTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
    }

    [Fact]
    public async Task PerformTransactionAsync_ConflictOnFirstAttempt_RetriesAndResolvesSafely()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        User user = CreateUser(order.UserId, startingBalance: 0m);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        // Simulate a concurrent PerformTransaction for the same Payme id committing between this call's
        // lookup and its own UpdateAsync: the first UpdateAsync attempt hits a conflict, and the row is
        // already Performed by the time the retry re-reads.
        transactionRepository.FaultOnUpdateAsync = callNumber =>
        {
            if (callNumber != 1)
            {
                return null;
            }

            PaymeTransaction concurrentlyPerformed = new()
            {
                Id = transaction.Id,
                PaymentOrderId = transaction.PaymentOrderId,
                PaymeTransactionId = transaction.PaymeTransactionId,
                AccountReference = transaction.AccountReference,
                PaymeTransactionTime = transaction.PaymeTransactionTime,
                RequestedAmountTiyin = transaction.RequestedAmountTiyin,
                MerchantCreateTime = transaction.MerchantCreateTime,
                MerchantPerformTime = 999_999,
                InternalStatus = PaymeTransactionInternalStatus.Performed,
                PaymeStateCode = 2
            };
            transactionRepository.SeedDirectly(concurrentlyPerformed);
            return new PaymeConflictException("simulated concurrent conflict");
        };

        PerformTransactionResponseDto response = await service.PerformTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(999_999, response.PerformTime);
        // The first attempt's balance credit must not survive: only the eventual, single successful
        // resolution (the idempotent replay path here) may leave the balance touched, and since the
        // "concurrent" writer here is simulated (not a real credit), this call's own attempt must not
        // have applied a second credit.
        Assert.Equal(0m, userRepository.GetRaw(order.UserId)!.Balance);
    }

    [Fact]
    public async Task PerformTransactionAsync_ConflictOnEveryAttempt_ThrowsInternalSystemErrorAfterRetriesExhausted()
    {
        PaymentOrder order = CreateOrder();
        User user = CreateUser(order.UserId);
        PaymeTransaction transaction = CreateCreatedTransaction(order);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        transactionRepository.FaultOnUpdateAsync = _ => new PaymeConflictException("always conflicts");

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.PerformTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.InternalSystemError, ex.PaymeErrorCode);
        // Every attempt applies then loses its own balance credit (rolled back with the rest of that
        // attempt's transaction in the real implementation) - the fake can't simulate a true rollback,
        // so this asserts only that no attempt was able to reach a committed, returned success state.
    }
}
