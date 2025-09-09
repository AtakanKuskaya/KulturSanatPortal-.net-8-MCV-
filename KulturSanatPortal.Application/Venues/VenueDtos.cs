namespace KulturSanatPortal.Application.Venues;

public record VenueDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";

    public int? Capacity { get; init; }
    public string? Address { get; init; }
    public string? District { get; init; }     // yeni
    public string? Phone { get; init; }        // yeni
    public string? Email { get; init; }        // yeni
    public string? MapEmbedUrl { get; init; }
    public string? DescriptionHtml { get; init; }
    public string? ImagePath { get; init; }    // yeni (kapak görseli)
}
