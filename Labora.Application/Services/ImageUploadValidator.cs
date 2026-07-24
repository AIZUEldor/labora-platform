using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public static class ImageUploadValidator
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public static void Validate(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Fayl bo'sh yoki mavjud emas.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("Fayl hajmi 5 MB dan oshmasligi kerak.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new ArgumentException("Faqat JPEG, PNG yoki WEBP formatidagi rasm yuklash mumkin.");

        string extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Faqat JPEG, PNG yoki WEBP formatidagi rasm yuklash mumkin.");
    }
}
