using System.Security.Cryptography;
using System.Text;
using Labora.Application.Interfaces;
using Labora.Application.Options;
using Microsoft.Extensions.Options;

namespace Labora.Application.Services;

/// <summary>
/// Validates the HTTP Basic Authorization header Payme sends on every Merchant API request, per the
/// confirmed "Authorization: Basic base64(login:password)" format from official documentation.
/// </summary>
public class PaymeAuthenticator : IPaymeAuthenticator
{
    private const string BasicScheme = "Basic ";

    private readonly IOptions<PaymeMerchantOptions> _options;

    public PaymeAuthenticator(IOptions<PaymeMerchantOptions> options)
    {
        _options = options;
    }

    public bool Validate(string? authorizationHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeaderValue) ||
            !authorizationHeaderValue.StartsWith(BasicScheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string encodedCredentials = authorizationHeaderValue[BasicScheme.Length..].Trim();

        byte[] actualBytes;
        try
        {
            actualBytes = Convert.FromBase64String(encodedCredentials);
        }
        catch (FormatException)
        {
            return false;
        }

        PaymeMerchantOptions options = _options.Value;
        byte[] expectedBytes = Encoding.UTF8.GetBytes($"{options.Login}:{options.Password}");

        // Compare the full "login:password" value as one fixed-time operation, rather than comparing
        // login and password separately, so a failed comparison never reveals via timing whether the
        // login portion alone was correct.
        return actualBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}
