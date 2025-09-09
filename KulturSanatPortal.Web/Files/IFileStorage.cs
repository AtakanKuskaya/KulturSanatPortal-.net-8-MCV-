using Microsoft.AspNetCore.Http;

namespace KulturSanatPortal.Web.Files;

public interface IFileStorage
{
    Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default);
    Task<bool> DeleteAsync(string? relativePath);
}
