using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushTokenRepository _pushTokenRepository;
    private static bool _firebaseInitialized = false;

    public PushNotificationService(IPushTokenRepository pushTokenRepository)
    {
        _pushTokenRepository = pushTokenRepository;
        InitializeFirebase();
    }

    private static void InitializeFirebase()
    {
        if (_firebaseInitialized || FirebaseApp.DefaultInstance is not null)
        {
            _firebaseInitialized = true;
            return;
        }

        try
        {
            string? firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");

            if (string.IsNullOrWhiteSpace(firebaseJson))
            {
                Console.WriteLine("Firebase service account env not found: FIREBASE_SERVICE_ACCOUNT_JSON");
                return;
            }

            GoogleCredential credential = GoogleCredential.FromJson(firebaseJson);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
            });

            _firebaseInitialized = true;
            Console.WriteLine("Firebase initialized successfully from environment variable.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Firebase initialization skipped: {ex.Message}");
        }
    }

    public async Task RegisterTokenAsync(Guid userId, string token, string? deviceType)
    {
        PushToken? existing = await _pushTokenRepository.GetByTokenAsync(token);
        if (existing is not null) return;

        await _pushTokenRepository.DeleteByUserIdAsync(userId);

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

        foreach (PushToken token in tokens)
        {
            await SendFcmAsync(token.Token, title, body, data);
        }
    }

    public async Task SendToUsersAsync(
        IEnumerable<Guid> userIds,
        string title,
        string body,
        object? data = null)
    {
        foreach (Guid userId in userIds)
        {
            await SendToUserAsync(userId, title, body, data);
        }
    }

    private static async Task SendFcmAsync(string deviceToken, string title, string body, object? data = null)
    {
        if (!_firebaseInitialized || FirebaseApp.DefaultInstance is null)
        {
            return;
        }

        try
        {
            Dictionary<string, string> dataDict = new();

            if (data is not null)
            {
                string json = JsonSerializer.Serialize(data);
                Dictionary<string, object>? parsed =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (parsed is not null)
                {
                    foreach (KeyValuePair<string, object> kv in parsed)
                    {
                        dataDict[kv.Key] = kv.Value?.ToString() ?? "";
                    }
                }
            }

            Message message = new()
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body,
                },
                Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                    },
                },
                Data = dataDict,
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch
        {
            // Push xatoligi ilovani to'xtatmasin
        }
    }
}