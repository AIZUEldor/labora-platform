using System.Net.Http.Json;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushTokenRepository _pushTokenRepository;
    private readonly HttpClient _httpClient;

    private const string ExpoApiUrl = "https://exp.host/--/api/v2/push/send";

    public PushNotificationService(
        IPushTokenRepository pushTokenRepository,
        IHttpClientFactory httpClientFactory)
    {
        _pushTokenRepository = pushTokenRepository;
        _httpClient = httpClientFactory.CreateClient("ExpoPush");
    }

    public async Task RegisterTokenAsync(Guid userId, string token, string? deviceType)
    {
        // Eski token bormi tekshir
        PushToken? existing = await _pushTokenRepository.GetByTokenAsync(token);
        if (existing is not null) return;

        // Foydalanuvchining eski tokenlarini o'chir
        await _pushTokenRepository.DeleteByUserIdAsync(userId);

        // Yangi token saqlash
        PushToken pushToken = new()
        {
            UserId = userId,
            Token = token,
            DeviceType = deviceType,
        };

        await _pushTokenRepository.AddAsync(pushToken);
    }

    public async Task RemoveTokenAsync(Guid userId)
    {
        await _pushTokenRepository.DeleteByUserIdAsync(userId);
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, object? data = null)
    {
        IEnumerable<PushToken> tokens = await _pushTokenRepository.GetByUserIdAsync(userId);

        List<object> messages = tokens.Select(t => (object)new
        {
            to = t.Token,
            title,
            body,
            data = data ?? new { },
            sound = "default",
            badge = 1,
        }).ToList();

        if (!messages.Any()) return;

        await SendBatchAsync(messages);
    }

    public async Task SendToUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string body,
        object? data = null)
    {
        List<object> messages = new();

        foreach (Guid userId in userIds)
        {
            IEnumerable<PushToken> tokens = await _pushTokenRepository.GetByUserIdAsync(userId);
            messages.AddRange(tokens.Select(t => (object)new
            {
                to = t.Token,
                title,
                body,
                data = data ?? new { },
                sound = "default",
                badge = 1,
            }));
        }

        if (!messages.Any()) return;

        await SendBatchAsync(messages);
    }

    private async Task SendBatchAsync(List<object> messages)
    {
        try
        {
            // Expo 100 ta limit — batch larga bo'lish
            int batchSize = 100;
            for (int i = 0; i < messages.Count; i += batchSize)
            {
                List<object> batch = messages.Skip(i).Take(batchSize).ToList();
                await _httpClient.PostAsJsonAsync(ExpoApiUrl, batch);
            }
        }
        catch
        {
            // Push xatoligi ilovani to'xtatmasin
        }
    }
}