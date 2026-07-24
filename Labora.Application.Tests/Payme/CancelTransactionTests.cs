using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Options;
using Labora.Application.Services;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;

namespace Labora.Application.Tests.Payme;

public class CancelTransactionTests
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

    private static PaymeTransaction CreateTransaction(
        PaymentOrder order,
        PaymeTransactionInternalStatus status,
        string paymeTransactionId = "payme-tx-1")
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
            MerchantPerformTime = status == PaymeTransactionInternalStatus.Performed ? 2 : null,
            InternalStatus = status,
            PaymeStateCode = status switch
            {
                PaymeTransactionInternalStatus.Performed => 2,
                PaymeTransactionInternalStatus.Cancelled => -1,
                _ => 1
            }
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

    private static CancelTransactionRequestDto Request(string id, int reason = 5) => new() { Id = id, Reason = reason };

    // ---- Cancel before perform (Created -> Cancelled) ----

    [Fact]
    public async Task CancelTransactionAsync_CreatedTransaction_TransitionsToCancelledWithStateMinusOne()
    {
        PaymentOrder order = CreateOrder();
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Created);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) =
            CreateService([order], [CreateUser(order.UserId)], [transaction]);

        CancelTransactionResponseDto response = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId, reason: 3));

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(PaymeTransactionInternalStatus.Cancelled, stored!.InternalStatus);
        Assert.Equal(-1, stored.PaymeStateCode);
        Assert.Equal(-1, response.State);
        Assert.NotNull(stored.MerchantCancelTime);
        Assert.Equal(stored.MerchantCancelTime, response.CancelTime);
        Assert.Equal(3, stored.CancelReason);
        Assert.Equal(stored.Id.ToString(), response.Transaction);
    }

    [Fact]
    public async Task CancelTransactionAsync_CreatedTransaction_TransitionsOrderPendingToCancelled()
    {
        PaymentOrder order = CreateOrder();
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Created);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, _, _) =
            CreateService([order], [CreateUser(order.UserId)], [transaction]);

        await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(PaymentOrderStatus.Cancelled, orderRepository.GetRaw(order.Id)!.Status);
    }

    [Fact]
    public async Task CancelTransactionAsync_CreatedTransaction_DoesNotTouchUserBalance()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        User user = CreateUser(order.UserId, startingBalance: 500m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Created);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(500m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
    }

    // ---- Cancel after perform (Performed -> Cancelled) ----

    [Fact]
    public async Task CancelTransactionAsync_PerformedTransactionSufficientBalance_ReversesExactCreditedAmount()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m, status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 15_000m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        CancelTransactionResponseDto response = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId, reason: 5));

        Assert.Equal(5_000m, userRepository.GetRaw(order.UserId)!.Balance);

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(PaymeTransactionInternalStatus.Cancelled, stored!.InternalStatus);
        Assert.Equal(-2, stored.PaymeStateCode);
        Assert.Equal(-2, response.State);
        Assert.Equal(5, stored.CancelReason);
        Assert.NotNull(stored.MerchantCancelTime);
        Assert.Equal(stored.MerchantCancelTime, response.CancelTime);
        Assert.Equal(PaymentOrderStatus.Cancelled, orderRepository.GetRaw(order.Id)!.Status);
    }

    [Fact]
    public async Task CancelTransactionAsync_PerformedTransactionBalanceExactlyEqualsAmount_SucceedsAndZeroesBalance()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m, status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 10_000m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(0m, userRepository.GetRaw(order.UserId)!.Balance);
    }

    [Fact]
    public async Task CancelTransactionAsync_PerformedTransactionInsufficientBalance_ThrowsOrderAlreadyCompleted_NoWrites()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m, status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 4_999m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.OrderAlreadyCompleted, ex.PaymeErrorCode);
        Assert.Equal(4_999m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
        Assert.Equal(0, transactionRepository.UpdateAsyncCallCount);
        Assert.Equal(PaymentOrderStatus.Paid, orderRepository.GetRaw(order.Id)!.Status);

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(PaymeTransactionInternalStatus.Performed, stored!.InternalStatus);
    }

    // ---- Idempotent replay ----

    [Fact]
    public async Task CancelTransactionAsync_AlreadyCancelledTransaction_ReturnsStoredResponse_IgnoresDifferentReason_NoWrites()
    {
        PaymentOrder order = CreateOrder();
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Cancelled);
        transaction.MerchantCancelTime = 123_456;
        transaction.PaymeStateCode = -1;
        transaction.CancelReason = 1;
        (PaymeMerchantService service, FakePaymentOrderRepository orderRepository, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [CreateUser(order.UserId, startingBalance: 777m)], [transaction]);

        CancelTransactionResponseDto response = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId, reason: 99));

        Assert.Equal(transaction.Id.ToString(), response.Transaction);
        Assert.Equal(123_456, response.CancelTime);
        Assert.Equal(-1, response.State);

        PaymeTransaction? stored = await transactionRepository.GetByPaymeTransactionIdAsync(transaction.PaymeTransactionId);
        Assert.Equal(1, stored!.CancelReason); // untouched, not overwritten with the replay's reason=99
        Assert.Equal(777m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(0, transactionRepository.UpdateAsyncCallCount);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
        Assert.Equal(PaymentOrderStatus.Pending, orderRepository.GetRaw(order.Id)!.Status);
    }

    [Fact]
    public async Task CancelTransactionAsync_PerformedThenCancelledTwice_DoesNotReverseBalanceTwice()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m, status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 10_000m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, _, _, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        CancelTransactionResponseDto first = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));
        CancelTransactionResponseDto second = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(first.Transaction, second.Transaction);
        Assert.Equal(first.CancelTime, second.CancelTime);
        Assert.Equal(first.State, second.State);
        Assert.Equal(0m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(1, userRepository.UpdateAsyncCallCount);
    }

    // ---- Not found / invalid states ----

    [Fact]
    public async Task CancelTransactionAsync_TransactionNotFound_ThrowsTransactionNotFound()
    {
        (PaymeMerchantService service, _, _, _) = CreateService([], [], []);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request("unknown-id")));

        Assert.Equal(PaymeErrorCodes.TransactionNotFound, ex.PaymeErrorCode);
    }

    [Theory]
    [InlineData(PaymentOrderStatus.Paid)]
    [InlineData(PaymentOrderStatus.Cancelled)]
    [InlineData(PaymentOrderStatus.Failed)]
    [InlineData(PaymentOrderStatus.Expired)]
    public async Task CancelTransactionAsync_CreatedTransactionOrderNotPending_ThrowsOperationNotAllowed_NoWrites(PaymentOrderStatus orderStatus)
    {
        PaymentOrder order = CreateOrder(status: orderStatus);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Created);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) =
            CreateService([order], [CreateUser(order.UserId)], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
        Assert.Equal(0, transactionRepository.UpdateAsyncCallCount);
    }

    [Theory]
    [InlineData(PaymentOrderStatus.Pending)]
    [InlineData(PaymentOrderStatus.Cancelled)]
    [InlineData(PaymentOrderStatus.Failed)]
    [InlineData(PaymentOrderStatus.Expired)]
    public async Task CancelTransactionAsync_PerformedTransactionOrderNotPaid_ThrowsOperationNotAllowed_NoWrites(PaymentOrderStatus orderStatus)
    {
        PaymentOrder order = CreateOrder(status: orderStatus);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [CreateUser(order.UserId, startingBalance: 999_999m)], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.OperationNotAllowed, ex.PaymeErrorCode);
        Assert.Equal(0, transactionRepository.UpdateAsyncCallCount);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
    }

    [Fact]
    public async Task CancelTransactionAsync_PerformedTransactionMissingUser_ThrowsInternalSystemError_NoWrites()
    {
        PaymentOrder order = CreateOrder(status: PaymentOrderStatus.Paid);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        // No user seeded for order.UserId.
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) =
            CreateService([order], [], [transaction]);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.InternalSystemError, ex.PaymeErrorCode);
        Assert.Equal(0, transactionRepository.UpdateAsyncCallCount);
    }

    // ---- Conflict / retry ----

    [Fact]
    public async Task CancelTransactionAsync_ConflictOnFirstAttempt_RetriesAndResolvesSafely_NoDoubleReversal()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m, status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 10_000m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, FakeUserRepository userRepository) =
            CreateService([order], [user], [transaction]);

        // Simulate a concurrent CancelTransaction for the same Payme id committing between this call's
        // lookup and its own UpdateAsync: the first UpdateAsync attempt hits a conflict, and the row is
        // already Cancelled by the time the retry re-reads.
        transactionRepository.FaultOnUpdateAsync = callNumber =>
        {
            if (callNumber != 1)
            {
                return null;
            }

            PaymeTransaction concurrentlyCancelled = new()
            {
                Id = transaction.Id,
                PaymentOrderId = transaction.PaymentOrderId,
                PaymeTransactionId = transaction.PaymeTransactionId,
                AccountReference = transaction.AccountReference,
                PaymeTransactionTime = transaction.PaymeTransactionTime,
                RequestedAmountTiyin = transaction.RequestedAmountTiyin,
                MerchantCreateTime = transaction.MerchantCreateTime,
                MerchantPerformTime = transaction.MerchantPerformTime,
                MerchantCancelTime = 999_999,
                InternalStatus = PaymeTransactionInternalStatus.Cancelled,
                PaymeStateCode = -2,
                CancelReason = 7
            };
            transactionRepository.SeedDirectly(concurrentlyCancelled);
            return new PaymeConflictException("simulated concurrent conflict");
        };

        CancelTransactionResponseDto response = await service.CancelTransactionAsync(Request(transaction.PaymeTransactionId));

        Assert.Equal(999_999, response.CancelTime);
        Assert.Equal(-2, response.State);
        // This call's own attempt must not have applied its own reversal on top of the "concurrent" one.
        Assert.Equal(10_000m, userRepository.GetRaw(order.UserId)!.Balance);
        Assert.Equal(0, userRepository.UpdateAsyncCallCount);
    }

    [Fact]
    public async Task CancelTransactionAsync_ConflictOnEveryAttempt_ThrowsInternalSystemError_NoLeakedInternalExceptionType()
    {
        PaymentOrder order = CreateOrder(status: PaymentOrderStatus.Paid);
        User user = CreateUser(order.UserId, startingBalance: 10_000m);
        PaymeTransaction transaction = CreateTransaction(order, PaymeTransactionInternalStatus.Performed);
        (PaymeMerchantService service, _, FakePaymeTransactionRepository transactionRepository, _) =
            CreateService([order], [user], [transaction]);

        transactionRepository.FaultOnUpdateAsync = _ => new PaymeConflictException("always conflicts");

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CancelTransactionAsync(Request(transaction.PaymeTransactionId)));

        Assert.Equal(PaymeErrorCodes.InternalSystemError, ex.PaymeErrorCode);
        Assert.IsType<PaymeRpcException>(ex);
    }
}
