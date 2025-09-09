namespace KulturSanatPortal.Domain.Entities;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public string BodyHtml { get; set; } = "";
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
    public string? HeroImagePath { get; set; }
    public bool IsPublished { get; set; } = true;
}
