using System.Security.Cryptography;
using System.Text;
using Labora.Application.Interfaces;
using Labora.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Labora.Application.Services;

public class OtpSecurityService : IOtpSecurityService
{
    private const int OtpCodeExclusiveUpperBound = 1_000_000;
    private const int OperationTokenSize = 32;
    private const int PepperSize = 32;

    private readonly byte[] _pepper;

    public OtpSecurityService(IConfiguration configuration)
    {
        string? pepper = configuration["Otp:Pepper"];

        if (string.IsNullOrWhiteSpace(pepper))
        {
            throw new InvalidOperationException("OTP pepper not configured.");
        }

        byte[] decodedPepper;

        try
        {
            decodedPepper = Convert.FromBase64String(pepper);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("OTP pepper is not valid base64.", ex);
        }

        if (decodedPepper.Length != PepperSize)
        {
            throw new InvalidOperationException("OTP pepper must decode to exactly 32 bytes (256 bits).");
        }

        _pepper = decodedPepper;
    }

    public string GenerateOtpCode()
    {
        int code = RandomNumberGenerator.GetInt32(0, OtpCodeExclusiveUpperBound);
        return code.ToString("D6");
    }

    public string GenerateOperationToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(OperationTokenSize);
        return Base64UrlEncode(bytes);
    }

    public string HashOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code)
    {
        return Convert.ToBase64String(ComputeOtpCodeHash(verificationId, purpose, normalizedPhone, code));
    }

    public bool VerifyOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code, string codeHash)
    {
        byte[] expected = ComputeOtpCodeHash(verificationId, purpose, normalizedPhone, code);
        return TryFixedTimeEquals(expected, codeHash);
    }

    public string HashOperationToken(Guid verificationId, string rawOperationToken)
    {
        return Convert.ToBase64String(ComputeOperationTokenHash(verificationId, rawOperationToken));
    }

    public bool VerifyOperationToken(Guid verificationId, string rawOperationToken, string operationTokenHash)
    {
        byte[] expected = ComputeOperationTokenHash(verificationId, rawOperationToken);
        return TryFixedTimeEquals(expected, operationTokenHash);
    }

    private byte[] ComputeOtpCodeHash(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code)
    {
        string context = $"OTP_CODE|{verificationId}|{purpose}|{normalizedPhone}|{code}";
        return ComputeHmac(context);
    }

    private byte[] ComputeOperationTokenHash(Guid verificationId, string rawOperationToken)
    {
        string context = $"OPERATION_TOKEN|{verificationId}|{rawOperationToken}";
        return ComputeHmac(context);
    }

    private byte[] ComputeHmac(string context)
    {
        return HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(context));
    }

    private static bool TryFixedTimeEquals(byte[] expected, string actualHash)
    {
        byte[] actual;

        try
        {
            actual = Convert.FromBase64String(actualHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
