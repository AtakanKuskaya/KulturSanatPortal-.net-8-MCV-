using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace KulturSanatPortal.Web.Files;

public class LocalFileStorage(IWebHostEnvironment env) : IFileStorage
{
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg",".jpeg",".png",".webp",".gif" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5MB

    public async Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0) throw new InvalidOperationException("Dosya boş.");
        if (file.Length > MaxBytes) throw new InvalidOperationException("Dosya boyutu 5MB'ı aşamaz.");

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext)) throw new InvalidOperationException("Yalnızca jpg,jpeg,png,webp,gif.");

        var now = DateTime.UtcNow;
        var logicalFolder = Path.Combine("uploads", subFolder, now.Year.ToString("0000"), now.Month.ToString("00"));
        var physicalFolder = Path.Combine(env.WebRootPath!, logicalFolder);
        Directory.CreateDirectory(physicalFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalFolder, fileName);

        using var fs = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(fs, ct);

        // web'de kullanılacak relative yol: "/uploads/..."
        var rel = "/" + Path.Combine(logicalFolder, fileName).Replace('\\', '/');
        return rel;
    }

    public Task<bool> DeleteAsync(string? relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);
            if (relativePath.StartsWith("/")) relativePath = relativePath[1..];
            var physical = Path.Combine(env.WebRootPath!, relativePath.Replace('/', '\\'));
            if (File.Exists(physical)) { File.Delete(physical); return Task.FromResult(true); }
            return Task.FromResult(false);
        }
        catch { return Task.FromResult(false); }
    }
}
