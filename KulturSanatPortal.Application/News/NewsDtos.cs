namespace KulturSanatPortal.Application.News
{
    public record NewsListItemDto(
        int Id,
        string Title,
        string Slug,
        DateTime PublishedLocal,
        string? Summary,
        string? HeroImagePath
    );

    public record NewsDetailsDto(
        int Id,
        string Title,
        string Slug,
        DateTime PublishedLocal,
        string? Summary,
        string? ContentHtml,     // <— TEK içerik alanı
        string? HeroImagePath
    );
}
