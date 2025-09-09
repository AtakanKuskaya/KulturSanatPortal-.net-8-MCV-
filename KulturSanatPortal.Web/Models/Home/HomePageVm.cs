namespace KulturSanatPortal.Web.Models.Home;

public record EventCard(
    int Id, string Title, string Slug,
    DateTime StartLocal, string VenueName, string CategoryName,
    string? Summary, string? HeroImagePath);

public record NewsCard(
    int Id, string Title, string Slug,
    DateTime PublishedLocal, string? Summary, string? HeroImagePath);

// --- MINI CALENDAR ---
public record MiniDay(DateTime DateLocal, bool InMonth, bool IsToday, int Count);
public class MiniCalendarVm
{
    public int Year { get; init; }
    public int Month { get; init; } // 1..12
    public List<MiniDay> Days { get; init; } = new(); // 6x7 = 42 hücre
}

public record CategoryBlock(int Id, string Name, string Slug, List<EventCard> Events);

public class HomePageVm
{
    public List<EventCard> Featured { get; init; } = new();
    public List<CategoryBlock> Categories { get; init; } = new();
    public List<NewsCard> News { get; init; } = new();
    public MiniCalendarVm Calendar { get; init; } = new();   // <— YENİ
}
