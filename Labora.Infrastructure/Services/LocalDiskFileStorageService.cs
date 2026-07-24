using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Labora.Infrastructure.Services;

public class LocalDiskFileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;
    private readonly ILogger<LocalDiskFileStorageService> _logger;

    public LocalDiskFileStorageService(ILogger<LocalDiskFileStorageService> logger)
        : this(logger, Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"))
    {
    }

    // Internal seam for tests: lets LocalDiskFileStorageServiceTests point at an isolated temp
    // directory instead of mutating the process-wide current directory, which would risk flaking
    // other tests running in parallel in this assembly.
    internal LocalDiskFileStorageService(ILogger<LocalDiskFileStorageService> logger, string uploadRoot)
    {
        _logger = logger;
        _uploadRoot = uploadRoot;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder)
    {
        string folder = Path.Combine(_uploadRoot, subFolder);
        Directory.CreateDirectory(folder);

        string extension = Path.GetExtension(file.FileName);
        string fileName = $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(folder, fileName);

        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public void Delete(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return;

        // relativeUrl is expected to look like "/uploads/portfolio/{guid}.jpg". Strip the known
        // prefix and re-resolve the full path via Path.GetFullPath, then require it to still be
        // inside _uploadRoot - this is what actually blocks a crafted value containing ".." from
        // escaping the upload root, not the prefix check alone.
        string trimmed = relativeUrl.TrimStart('/');
        const string prefix = "uploads/";
        if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Refused to delete a path outside the uploads prefix: {RelativeUrl}", relativeUrl);
            return;
        }

        string relativePath = trimmed[prefix.Length..];
        string fullPath = Path.GetFullPath(Path.Combine(_uploadRoot, relativePath));
        string normalizedRoot = Path.GetFullPath(_uploadRoot) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Refused to delete a path outside the upload root: {RelativeUrl}", relativeUrl);
            return;
        }

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            // A missing file is not an error here - the caller's DB state is already the source of
            // truth, and callers must not fail an already-completed operation over a physical file
            // that was already gone (deleted manually, never written, etc).
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {FullPath}", fullPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {FullPath}", fullPath);
        }
    }
}
