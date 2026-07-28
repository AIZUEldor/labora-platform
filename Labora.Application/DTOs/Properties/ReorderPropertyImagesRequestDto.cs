namespace Labora.Application.DTOs.Properties;

public class ReorderPropertyImagesRequestDto
{
    public List<Guid> ImageIds { get; set; } = new();
}
