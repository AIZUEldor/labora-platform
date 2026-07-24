using System.Text;
using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Services;

namespace Labora.Application.Tests.Payme;

public class PaymeCheckoutUrlBuilderTests
{
    private readonly PaymeCheckoutUrlBuilder _builder = new();

    [Fact]
    public void Build_OfficialDocumentationSample_ReproducesExactUrl()
    {
        // Reproduces the exact example from developer.help.paycom.uz/initsializatsiya-platezhey/
        // (verified by decoding the sample: "m=587f72c72cac0d162c722ae2;ac.order_id=197;a=500").
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "587f72c72cac0d162c722ae2",
            AccountFieldName = "order_id",
            AccountValue = "197",
            AmountInTiyin = 500
        };

        string url = _builder.Build(request);

        Assert.Equal(
            "https://checkout.paycom.uz/bT01ODdmNzJjNzJjYWMwZDE2MmM3MjJhZTI7YWMub3JkZXJfaWQ9MTk3O2E9NTAw",
            url);
    }

    [Fact]
    public void Build_SameInput_IsDeterministic()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "abc-123",
            AmountInTiyin = 100_000,
            Language = "uz",
            ReturnUrl = "https://alpjobs.uz/payment/return",
            RedirectTimeoutMilliseconds = 5000,
            CurrencyCode = "860"
        };

        string first = _builder.Build(request);
        string second = _builder.Build(request);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Build_NonPositiveAmount_ThrowsArgumentOutOfRangeException(long amount)
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "abc-123",
            AmountInTiyin = amount
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _builder.Build(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_MissingMerchantId_ThrowsArgumentException(string? merchantId)
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = merchantId!,
            AccountFieldName = "order_id",
            AccountValue = "abc-123",
            AmountInTiyin = 100
        };

        Assert.Throws<ArgumentException>(() => _builder.Build(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_MissingAccountFieldName_ThrowsArgumentException(string? accountFieldName)
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = accountFieldName!,
            AccountValue = "abc-123",
            AmountInTiyin = 100
        };

        Assert.Throws<ArgumentException>(() => _builder.Build(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Build_MissingAccountValue_ThrowsArgumentException(string? accountValue)
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = accountValue!,
            AmountInTiyin = 100
        };

        Assert.Throws<ArgumentException>(() => _builder.Build(request));
    }

    [Fact]
    public void Build_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.Build(null!));
    }

    [Fact]
    public void Build_ValueContainingParameterSeparator_ThrowsArgumentException()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "abc-123",
            AmountInTiyin = 100,
            ReturnUrl = "https://alpjobs.uz/return;evil=1"
        };

        Assert.Throws<ArgumentException>(() => _builder.Build(request));
    }

    [Fact]
    public void Build_AllOptionalParametersProvided_AreIncludedInDeterministicOrder()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "197",
            AmountInTiyin = 500,
            Language = "ru",
            ReturnUrl = "https://alpjobs.uz/return",
            RedirectTimeoutMilliseconds = 15000,
            CurrencyCode = "860"
        };

        string decoded = Decode(_builder.Build(request));

        Assert.Equal(
            "m=merchant-1;ac.order_id=197;a=500;l=ru;c=https://alpjobs.uz/return;ct=15000;cr=860",
            decoded);
    }

    [Fact]
    public void Build_NoOptionalParametersProvided_OmitsThemEntirely()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "197",
            AmountInTiyin = 500
        };

        string decoded = Decode(_builder.Build(request));

        Assert.Equal("m=merchant-1;ac.order_id=197;a=500", decoded);
        Assert.DoesNotContain("l=", decoded);
        Assert.DoesNotContain("c=", decoded);
        Assert.DoesNotContain("ct=", decoded);
        Assert.DoesNotContain("cr=", decoded);
    }

    [Fact]
    public void Build_NonAsciiAccountValue_IsUtf8EncodedBeforeBase64()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "merchant-1",
            AccountFieldName = "order_id",
            AccountValue = "Тест-197",
            AmountInTiyin = 500
        };

        string decoded = Decode(_builder.Build(request));

        Assert.Equal("m=merchant-1;ac.order_id=Тест-197;a=500", decoded);
    }

    [Fact]
    public void Build_Url_DoesNotContainAnySecretLikeValue()
    {
        PaymeCheckoutUrlRequest request = new()
        {
            MerchantId = "587f72c72cac0d162c722ae2",
            AccountFieldName = "order_id",
            AccountValue = "197",
            AmountInTiyin = 500
        };

        string url = _builder.Build(request);

        // The builder's own input surface has no secret/key field at all - this asserts the
        // checkout URL only ever encodes the public parameters it was given.
        Assert.StartsWith("https://checkout.paycom.uz/", url);
        Assert.DoesNotContain("secret", url, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("key", url, StringComparison.OrdinalIgnoreCase);
    }

    private static string Decode(string checkoutUrl)
    {
        string base64Segment = checkoutUrl["https://checkout.paycom.uz/".Length..];
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64Segment));
    }
}
