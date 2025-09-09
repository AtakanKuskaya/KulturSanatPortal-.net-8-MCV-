namespace KulturSanatPortal.Domain.Entities;

public class EventCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
