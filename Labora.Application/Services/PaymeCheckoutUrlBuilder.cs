using System.Text;
using Labora.Application.DTOs.Payments.Payme;
using Labora.Application.Interfaces;

namespace Labora.Application.Services;

/// <summary>
/// Builds a Payme checkout URL per the confirmed GET-method format:
/// https://checkout.paycom.uz/base64("m=...;ac.field=...;a=...[;l=...][;c=...][;ct=...][;cr=...]").
/// Pure string construction only - no network call, and no provider secret is ever included (only
/// the public merchant/cash-register id).
/// </summary>
public class PaymeCheckoutUrlBuilder : IPaymeCheckoutUrlBuilder
{
    private const string CheckoutBaseUrl = "https://checkout.paycom.uz/";

    public string Build(PaymeCheckoutUrlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.MerchantId))
            throw new ArgumentException("Merchant id is required.", nameof(request));

        if (request.AmountInTiyin <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), request.AmountInTiyin, "Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.AccountFieldName))
            throw new ArgumentException("Account field name is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.AccountValue))
            throw new ArgumentException("Account value is required.", nameof(request));

        EnsureNoParameterSeparator(request.MerchantId, nameof(request.MerchantId));
        EnsureNoParameterSeparator(request.AccountFieldName, nameof(request.AccountFieldName));
        EnsureNoParameterSeparator(request.AccountValue, nameof(request.AccountValue));
        EnsureNoParameterSeparator(request.Language, nameof(request.Language));
        EnsureNoParameterSeparator(request.ReturnUrl, nameof(request.ReturnUrl));
        EnsureNoParameterSeparator(request.CurrencyCode, nameof(request.CurrencyCode));

        List<string> parameters = new()
        {
            $"m={request.MerchantId}",
            $"ac.{request.AccountFieldName}={request.AccountValue}",
            $"a={request.AmountInTiyin}"
        };

        if (!string.IsNullOrWhiteSpace(request.Language))
            parameters.Add($"l={request.Language}");

        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
            parameters.Add($"c={request.ReturnUrl}");

        if (request.RedirectTimeoutMilliseconds is not null)
            parameters.Add($"ct={request.RedirectTimeoutMilliseconds}");

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
            parameters.Add($"cr={request.CurrencyCode}");

        string joined = string.Join(';', parameters);
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(joined));

        return CheckoutBaseUrl + base64;
    }

    private static void EnsureNoParameterSeparator(string? value, string paramName)
    {
        if (value is not null && value.Contains(';'))
            throw new ArgumentException("Value must not contain ';', which is the Payme checkout parameter separator.", paramName);
    }
}
