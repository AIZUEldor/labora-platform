using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Options;
using Labora.Application.Services;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;

namespace Labora.Application.Tests.Payme;

public class CheckPerformTransactionTests
{
    private static PaymentOrder CreateOrder(
        decimal amountSoum = 10_000m,
        PaymentProvider provider = PaymentProvider.Payme,
        PaymentOrderStatus status = PaymentOrderStatus.Pending,
        DateTime? expiresAt = null)
    {
        return new PaymentOrder
        {
            Id = Guid.NewGuid(),
            Amount = amountSoum,
            Provider = provider,
            Status = status,
            UserId = Guid.NewGuid(),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(30)
        };
    }

    private static PaymeMerchantService CreateService(params PaymentOrder[] seedOrders)
    {
        FakePaymentOrderRepository orderRepository = new(seedOrders);
        PaymeAuthenticator authenticator = new(Microsoft.Extensions.Options.Options.Create(new PaymeMerchantOptions
        {
            MerchantId = "merchant-1",
            Login = "Paycom",
            Password = "secret"
        }));

        return new PaymeMerchantService(authenticator, orderRepository, new FakePaymeTransactionRepository(), new PassThroughUnitOfWork());
    }

    [Fact]
    public async Task CheckPerformTransactionAsync_ValidPendingOrderMatchingAmount_ReturnsAllowTrue()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        PaymeMerchantService service = CreateService(order);

        CheckPerformTransactionResponseDto response = await service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
        {
            Amount = 1_000_000, // 10,000 so'm * 100
            Account = new PaymeAccountDto { OrderId = order.Id.ToString() }
        });

        Assert.True(response.Allow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid")]
    public async Task CheckPerformTransactionAsync_MalformedOrderId_ThrowsAccountError(string orderId)
    {
        PaymeMerchantService service = CreateService();

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 1_000_000,
                Account = new PaymeAccountDto { OrderId = orderId }
            }));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CheckPerformTransactionAsync_OrderNotFound_ThrowsAccountError()
    {
        PaymeMerchantService service = CreateService();

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 1_000_000,
                Account = new PaymeAccountDto { OrderId = Guid.NewGuid().ToString() }
            }));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CheckPerformTransactionAsync_WrongProvider_ThrowsAccountError()
    {
        PaymentOrder order = CreateOrder(provider: PaymentProvider.Click);
        PaymeMerchantService service = CreateService(order);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 1_000_000,
                Account = new PaymeAccountDto { OrderId = order.Id.ToString() }
            }));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
    }

    [Theory]
    [InlineData(PaymentOrderStatus.Paid)]
    [InlineData(PaymentOrderStatus.Cancelled)]
    [InlineData(PaymentOrderStatus.Failed)]
    [InlineData(PaymentOrderStatus.Expired)]
    public async Task CheckPerformTransactionAsync_NonPendingOrder_ThrowsAccountError(PaymentOrderStatus status)
    {
        PaymentOrder order = CreateOrder(status: status);
        PaymeMerchantService service = CreateService(order);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 1_000_000,
                Account = new PaymeAccountDto { OrderId = order.Id.ToString() }
            }));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CheckPerformTransactionAsync_ExpiredPendingOrder_ThrowsAccountError()
    {
        PaymentOrder order = CreateOrder(expiresAt: DateTime.UtcNow.AddMinutes(-5));
        PaymeMerchantService service = CreateService(order);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 1_000_000,
                Account = new PaymeAccountDto { OrderId = order.Id.ToString() }
            }));

        Assert.Equal(PaymeErrorCodes.AccountErrorRangeStart, ex.PaymeErrorCode);
    }

    [Fact]
    public async Task CheckPerformTransactionAsync_AmountMismatch_ThrowsInvalidAmount()
    {
        PaymentOrder order = CreateOrder(amountSoum: 10_000m);
        PaymeMerchantService service = CreateService(order);

        PaymeRpcException ex = await Assert.ThrowsAsync<PaymeRpcException>(() =>
            service.CheckPerformTransactionAsync(new CheckPerformTransactionRequestDto
            {
                Amount = 999_999,
                Account = new PaymeAccountDto { OrderId = order.Id.ToString() }
            }));

        Assert.Equal(PaymeErrorCodes.InvalidAmount, ex.PaymeErrorCode);
    }
}
