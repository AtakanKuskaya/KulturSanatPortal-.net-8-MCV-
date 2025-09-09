namespace KulturSanatPortal.Web.Utils;

public static class WebPathExtensions
{
    /// <summary>
    /// DB'de tutulan dosya yolunu (\\ sorunlarını düzelterek) Url.Content'e uygun hale getirir.
    /// http/https ise aynen döner, / ile başlıyorsa başına ~ ekler.
    /// </summary>
    public static string ToContentUrl(this string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        var p = path.Replace("\\", "/");

        if (p.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
            return p;                    // tam URL

        if (p.StartsWith("~/"))
            return p;                    // zaten content-root

        if (p.StartsWith("/"))
            return "~" + p;              // /uploads/...  -> ~/uploads/...

        return "~/" + p;                 // uploads/...   -> ~/uploads/...
    }
}
