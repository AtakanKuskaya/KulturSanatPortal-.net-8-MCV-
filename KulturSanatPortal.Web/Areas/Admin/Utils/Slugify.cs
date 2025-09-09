using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers.Utils;

public static class Slugify
{
    public static string From(string input)
    {
        input = (input ?? "").ToLowerInvariant().Trim();
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        var cleaned = Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"[^a-z0-9\s-]", "");
        cleaned = Regex.Replace(cleaned, @"\s+", "-").Trim('-');
        cleaned = Regex.Replace(cleaned, "-{2,}", "-");
        return string.IsNullOrWhiteSpace(cleaned) ? Guid.NewGuid().ToString("n")[..8] : cleaned;
    }
}
