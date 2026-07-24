namespace Labora.Application.DTOs.Jobs;

public class JobImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
}
