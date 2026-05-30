using Labora.Application.DTOs.Notifications;
using Labora.Domain.Enums;

namespace Labora.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? referenceId = null);
    Task SendJobRecommendationsAsync();
    Task SavePreferencesAsync(Guid userId, UserPreferenceRequestDto dto);
    Task<IEnumerable<UserPreferenceRequestDto>> GetPreferencesAsync(Guid userId);
}