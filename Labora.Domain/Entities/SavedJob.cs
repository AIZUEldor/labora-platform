namespace Labora.Domain.Entities;

public class SavedJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}