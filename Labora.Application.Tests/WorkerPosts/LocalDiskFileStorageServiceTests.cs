using Labora.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Labora.Application.Tests.WorkerPosts;

public class LocalDiskFileStorageServiceTests
{
    private static LocalDiskFileStorageService CreateService(string uploadRoot)
        => new(NullLogger<LocalDiskFileStorageService>.Instance, uploadRoot);

    [Fact]
    public void Delete_MissingPhysicalFile_DoesNotThrow()
    {
        string uploadRoot = Path.Combine(Path.GetTempPath(), "labora-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(uploadRoot);
        LocalDiskFileStorageService service = CreateService(uploadRoot);

        Exception? exception = Record.Exception(() => service.Delete("/uploads/portfolio/does-not-exist.jpg"));

        Assert.Null(exception);
    }

    [Fact]
    public void Delete_ExistingFile_RemovesIt()
    {
        string uploadRoot = Path.Combine(Path.GetTempPath(), "labora-tests-" + Guid.NewGuid());
        string portfolioFolder = Path.Combine(uploadRoot, "portfolio");
        Directory.CreateDirectory(portfolioFolder);
        string filePath = Path.Combine(portfolioFolder, "existing.jpg");
        File.WriteAllBytes(filePath, new byte[] { 1, 2, 3 });
        LocalDiskFileStorageService service = CreateService(uploadRoot);

        service.Delete("/uploads/portfolio/existing.jpg");

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void Delete_PathTraversalAttempt_DoesNotDeleteOutsideUploadRoot()
    {
        string uploadRoot = Path.Combine(Path.GetTempPath(), "labora-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(uploadRoot);
        string outsideFile = Path.Combine(Path.GetTempPath(), "labora-tests-outside-" + Guid.NewGuid() + ".txt");
        File.WriteAllBytes(outsideFile, new byte[] { 1 });
        LocalDiskFileStorageService service = CreateService(uploadRoot);

        try
        {
            service.Delete("/uploads/../../" + Path.GetFileName(outsideFile));

            Assert.True(File.Exists(outsideFile));
        }
        finally
        {
            File.Delete(outsideFile);
        }
    }
}
