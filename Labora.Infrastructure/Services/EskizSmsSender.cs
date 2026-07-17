using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Labora.Application.Interfaces;
using Labora.Domain.Exceptions;
using Labora.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Labora.Infrastructure.Services;

/// <summary>
/// Eskiz.uz ISmsSender implementation. Registered as a Singleton (see DependencyInjection.cs) so its
/// process-memory token cache survives across requests; a Scoped/Transient lifetime - or a typed
/// HttpClient holding the cache itself - would silently reset it on every resolution. The actual
/// HttpClient still comes from IHttpClientFactory on every call, so connection pooling/DNS refresh
/// is unaffected by this instance's own long lifetime.
/// </summary>
public sealed class EskizSmsSender : ISmsSender, IDisposable
{
    public const string HttpClientName = "Eskiz";

    private const string EskizCountryCode = "998";
    private const int EskizPhoneLength = 12;

    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<EskizOptions> _options;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private volatile string? _cachedToken;

    public EskizSmsSender(IHttpClientFactory httpClientFactory, IOptions<EskizOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new SmsSendException("SMS message must not be empty.");
        }

        string eskizPhone = ToEskizPhoneFormat(phoneNumber);
        string token = await GetTokenAsync();

        if (await TrySendAsync(eskizPhone, message, token))
        {
            return;
        }

        // Exactly one re-authentication + resend is allowed, and only for a confirmed 401 - every
        // other failure mode (timeout, network error, 408/429/5xx, parse failure, unclear provider
        // error) throws directly from TrySendAsync without ever reaching this point, because the
        // provider may already have accepted the message and a blind retry risks a duplicate SMS.
        string freshToken = await RefreshTokenAsync(token);

        if (!await TrySendAsync(eskizPhone, message, freshToken))
        {
            throw new SmsSendException("SMS provider rejected the request after re-authentication.");
        }
    }

    private async Task<bool> TrySendAsync(string eskizPhone, string message, string token)
    {
        HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

        using HttpRequestMessage request = new(HttpMethod.Post, "api/message/sms/send")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["mobile_phone"] = eskizPhone,
                ["message"] = message,
                ["from"] = _options.Value.SenderName,
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Network/timeout failures are never retried here: whether the provider received and
            // processed the request before the failure is unknown, so retrying risks a duplicate SMS.
            throw new SmsSendException("Failed to reach the SMS provider.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new SmsSendException($"SMS provider returned an unexpected status: {(int)response.StatusCode}.");
            }

            EskizSendResponse? parsed;
            try
            {
                string body = await response.Content.ReadAsStringAsync();
                parsed = JsonSerializer.Deserialize<EskizSendResponse>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new SmsSendException("SMS provider response could not be parsed.", ex);
            }

            // Per Eskiz's documented contract, a successfully accepted send returns both a tracking
            // id AND status "waiting" (its async gateway queues the message rather than confirming
            // delivery synchronously). Either field being absent or status holding any other value
            // means the provider did not confirm acceptance even though the HTTP call itself returned
            // success - this is the "no fail-open on HTTP 200 with a failure body" check.
            if (parsed is null
                || string.IsNullOrWhiteSpace(parsed.Id)
                || !string.Equals(parsed.Status, "waiting", StringComparison.OrdinalIgnoreCase))
            {
                throw new SmsSendException("SMS provider did not confirm the message was accepted.");
            }

            return true;
        }
    }

    private Task<string> GetTokenAsync()
    {
        string? cached = _cachedToken;
        return cached is not null ? Task.FromResult(cached) : AcquireTokenAsync();
    }

    private async Task<string> AcquireTokenAsync()
    {
        await _tokenLock.WaitAsync();
        try
        {
            if (_cachedToken is not null)
            {
                return _cachedToken;
            }

            string token = await LoginAsync();
            _cachedToken = token;
            return token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> RefreshTokenAsync(string staleToken)
    {
        await _tokenLock.WaitAsync();
        try
        {
            if (_cachedToken is not null && _cachedToken != staleToken)
            {
                // A concurrent call already refreshed the token while this one waited for the lock;
                // reuse it instead of invalidating a token that is already current.
                return _cachedToken;
            }

            string token = await LoginAsync();
            _cachedToken = token;
            return token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> LoginAsync()
    {
        EskizOptions options = _options.Value;
        HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

        using HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = options.Email,
            ["password"] = options.Password,
        });

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("api/auth/login", content);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new SmsSendException("Failed to reach the SMS provider for authentication.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new SmsSendException($"SMS provider authentication failed with status {(int)response.StatusCode}.");
            }

            EskizLoginResponse? parsed;
            try
            {
                string body = await response.Content.ReadAsStringAsync();
                parsed = JsonSerializer.Deserialize<EskizLoginResponse>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new SmsSendException("SMS provider authentication response could not be parsed.", ex);
            }

            string? token = parsed?.Data?.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SmsSendException("SMS provider authentication response did not include a token.");
            }

            return token;
        }
    }

    /// <summary>
    /// Converts the already-normalized +998XXXXXXXXX form (see IPhoneNumberNormalizer) to the bare
    /// 998XXXXXXXXX digits Eskiz expects. A malformed input at this point indicates an inconsistency
    /// upstream of this class, not a provider failure, but it is still surfaced as a typed
    /// SmsSendException rather than ever reaching the provider with a bad value.
    /// </summary>
    private static string ToEskizPhoneFormat(string normalizedPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhoneNumber))
        {
            throw new SmsSendException("Phone number must not be empty.");
        }

        string digits = normalizedPhoneNumber.StartsWith('+') ? normalizedPhoneNumber[1..] : normalizedPhoneNumber;

        if (digits.Length != EskizPhoneLength || !digits.StartsWith(EskizCountryCode) || !digits.All(char.IsAsciiDigit))
        {
            throw new SmsSendException("Phone number is not in a format the SMS provider accepts.");
        }

        return digits;
    }

    public void Dispose() => _tokenLock.Dispose();

    private sealed class EskizLoginResponse
    {
        [JsonPropertyName("data")]
        public EskizLoginData? Data { get; set; }
    }

    private sealed class EskizLoginData
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    private sealed class EskizSendResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
