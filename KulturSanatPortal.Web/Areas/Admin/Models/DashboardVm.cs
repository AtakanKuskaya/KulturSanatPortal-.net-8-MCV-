namespace KulturSanatPortal.Web.Areas.Admin.Models;

public sealed record DashboardVm(
    int TotalEvents,
    int MonthEvents,
    int VenueCount,
    int NewsCount,
    IReadOnlyList<MiniEvent> Upcoming,
    IReadOnlyList<MiniEvent> RecentEvents,
    IReadOnlyList<MiniNews> RecentNews,
    IReadOnlyList<MiniVenue> RecentVenues
);

public sealed record MiniEvent(
    int Id, string Title, DateTime StartLocal, string VenueName, bool IsPublished
);

public sealed record MiniNews(
    int Id, string Title, DateTime PublishedLocal, bool IsPublished
);

public sealed record MiniVenue(
    int Id, string Name
);
