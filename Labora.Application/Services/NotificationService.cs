using Labora.Application.DTOs.Notifications;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserPreferenceRepository _preferenceRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;

    private readonly IPushNotificationService _pushNotificationService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserPreferenceRepository preferenceRepository,
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IPushNotificationService pushNotificationService)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId)
    {
        IEnumerable<Notification> notifications =
            await _notificationRepository.GetUserNotificationsAsync(userId);

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            ReferenceId = n.ReferenceId,
            CreatedAt = n.CreatedAt,
        });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _notificationRepository.GetUnreadCountAsync(userId);

    public async Task MarkAsReadAsync(Guid notificationId)
        => await _notificationRepository.MarkAsReadAsync(notificationId);

    public async Task MarkAllAsReadAsync(Guid userId)
        => await _notificationRepository.MarkAllAsReadAsync(userId);

    public async Task CreateAsync(
     Guid userId,
     string title,
     string message,
     NotificationType type,
     Guid? referenceId = null)
    {
        Notification notification = new()
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            IsRead = false,
        };

        await _notificationRepository.AddAsync(notification);

        // Push notification yuborish
        await _pushNotificationService.SendToUserAsync(userId, title, message, new
        {
            type = (int)type,
            referenceId = referenceId?.ToString(),
        });
    }

    public async Task SendJobRecommendationsAsync()
    {
        IEnumerable<User> workers = await _userRepository.GetWorkerUsersAsync();

        foreach (User worker in workers)
        {
            IEnumerable<UserPreference> preferences =
                await _preferenceRepository.GetByUserIdAsync(worker.Id);

            IEnumerable<Job> recommendedJobs =
                await _jobRepository.GetRecommendedJobsAsync(
                    worker.Latitude,
                    worker.Longitude,
                    preferences);

            foreach (Job job in recommendedJobs.Take(3))
            {
                await CreateAsync(
                    worker.Id,
                    "Yangi ish tavsiyasi",
                    $"{job.Title} — {job.City} — {job.Salary:N0} so'm",
                    NotificationType.NewJobRecommended,
                    job.Id);
            }
        }
    }

    public async Task SavePreferencesAsync(Guid userId, UserPreferenceRequestDto dto)
    {
        await _preferenceRepository.DeleteByUserIdAsync(userId);

        foreach (Guid categoryId in dto.PreferredCategoryIds)
        {
            UserPreference preference = new()
            {
                UserId = userId,
                PreferredCategoryId = categoryId,
                PreferredJobType = dto.PreferredJobType,
                MaxDistanceKm = dto.MaxDistanceKm,
            };

            await _preferenceRepository.AddAsync(preference);
        }
    }

    public async Task<IEnumerable<UserPreferenceRequestDto>> GetPreferencesAsync(Guid userId)
    {
        IEnumerable<UserPreference> preferences =
            await _preferenceRepository.GetByUserIdAsync(userId);

        return preferences.Select(p => new UserPreferenceRequestDto
        {
            PreferredCategoryIds = new List<Guid>
            {
                p.PreferredCategoryId ?? Guid.Empty
            },
            PreferredJobType = p.PreferredJobType,
            MaxDistanceKm = p.MaxDistanceKm,
        });
    }
}