namespace KulturSanatPortal.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public string? DescriptionHtml { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; } = false;

    public int VenueId { get; set; }
    public Venue Venue { get; set; } = default!;

    public int CategoryId { get; set; }
    public EventCategory Category { get; set; } = default!;

    public string? TicketUrl { get; set; }
    public string? PriceInfo { get; set; }
    // NEW
    public string? HeroImagePath { get; set; }    // Kapak görseli
    public ICollection<EventImage> Images { get; set; } = new List<EventImage>(); // Galeri
}

public class EventImage
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    public string ImagePath { get; set; } = "";
    public int SortOrder { get; set; } = 0;
    public string? Caption { get; set; }
}