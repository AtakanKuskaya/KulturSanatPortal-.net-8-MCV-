namespace KulturSanatPortal.Application.Dashboard;

public record AdminCounters(int TotalEvents, int ThisMonthEvents, int Venues, int News);

public record AdminEventRow(int Id, string Title, DateTime StartLocal, string? Venue, bool IsPublished);
public record AdminNewsRow(int Id, string Title, DateTime PublishedLocal);
public record AdminVenueRow(int Id, string Name);

public record AdminDashboardVm(
    AdminCounters Counters,
    IReadOnlyList<AdminEventRow> Upcoming,
    IReadOnlyList<AdminEventRow> RecentEvents,
    IReadOnlyList<AdminNewsRow> RecentNews,
    IReadOnlyList<AdminVenueRow> Venues
);
