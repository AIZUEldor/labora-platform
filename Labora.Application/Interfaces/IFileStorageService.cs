using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string subFolder);
    void Delete(string relativeUrl);
}
