using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? ProfileImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal Balance { get; set; } = 0;
    public bool IsVerified { get; set; } = false;
    public bool IsBlocked { get; set; } = false;
    public string? CvUrl { get; set; }

    // Navigation properties
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    public ICollection<PushToken> PushTokens { get; set; } = new List<PushToken>();
}