namespace KulturSanatPortal.Domain.Entities;

public class Venue
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? Capacity { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ImagePath { get; set; }
    public string? MapEmbedUrl { get; set; }
    public string? DescriptionHtml { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
