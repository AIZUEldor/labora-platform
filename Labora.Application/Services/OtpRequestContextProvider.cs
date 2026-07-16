using System.Net;
using Labora.Application.Common;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class OtpRequestContextProvider : IOtpRequestContextProvider
{
    private const string DeviceIdHeaderName = "X-Device-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentifierHasher _identifierHasher;

    public OtpRequestContextProvider(IHttpContextAccessor httpContextAccessor, IIdentifierHasher identifierHasher)
    {
        _httpContextAccessor = httpContextAccessor;
        _identifierHasher = identifierHasher;
    }

    public OtpRequestContext GetContext()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        return new OtpRequestContext
        {
            IpHash = ResolveIpHash(httpContext),
            DeviceHash = ResolveDeviceHash(httpContext)
        };
    }

    private string? ResolveIpHash(HttpContext? httpContext)
    {
        IPAddress? remoteIp = httpContext?.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            return null;
        }

        IPAddress normalizedIp = remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4() : remoteIp;

        return _identifierHasher.HashIp(normalizedIp.ToString());
    }

    private string? ResolveDeviceHash(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        string? rawDeviceId = httpContext.Request.Headers[DeviceIdHeaderName].FirstOrDefault();

        return string.IsNullOrWhiteSpace(rawDeviceId) ? null : _identifierHasher.HashDevice(rawDeviceId);
    }
}
