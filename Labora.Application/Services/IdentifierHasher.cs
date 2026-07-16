using System.Security.Cryptography;
using System.Text;
using Labora.Application.Interfaces;
using Labora.Application.Options;
using Microsoft.Extensions.Options;

namespace Labora.Application.Services;

public class IdentifierHasher : IIdentifierHasher
{
    private const int PepperSize = 32;
    private const string IpPrefix = "IP|";
    private const string DevicePrefix = "DEVICE|";

    private readonly byte[] _pepper;

    public IdentifierHasher(IOptions<IdentifierHashOptions> options)
    {
        byte[] decodedPepper;

        try
        {
            decodedPepper = Convert.FromBase64String(options.Value.Pepper);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("OtpIdentity pepper is not valid base64.", ex);
        }

        if (decodedPepper.Length != PepperSize)
        {
            throw new InvalidOperationException("OtpIdentity pepper must decode to exactly 32 bytes (256 bits).");
        }

        _pepper = decodedPepper;
    }

    public string HashIp(string rawIp)
    {
        return ComputeHash(IpPrefix, rawIp);
    }

    public string HashDevice(string rawDeviceId)
    {
        return ComputeHash(DevicePrefix, rawDeviceId);
    }

    private string ComputeHash(string prefix, string rawValue)
    {
        byte[] hash = HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(prefix + rawValue));
        return Convert.ToBase64String(hash);
    }
}
