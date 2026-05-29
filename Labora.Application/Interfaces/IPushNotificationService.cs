namespace Labora.Application.Interfaces;

public interface IPushNotificationService
{
    Task RegisterTokenAsync(Guid userId, string token, string? deviceType);
    Task RemoveTokenAsync(Guid userId);
    Task SendToUserAsync(Guid userId, string title, string body, object? data = null);
    Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, object? data = null);
}