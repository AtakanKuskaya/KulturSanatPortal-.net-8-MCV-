using KulturSanatPortal.Application.Dashboard;
using KulturSanatPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Infrastructure.Services;

public class DashboardReadService(AppDbContext db) : IDashboardReadService
{
    private static TimeZoneInfo Tz =>
        TimeZoneInfo.GetSystemTimeZones().Any(z => z.Id == "Turkey Standard Time")
            ? TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public async Task<AdminDashboardVm> GetAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, Tz);

        var startMonthLocal = new DateTime(nowLocal.Year, nowLocal.Month, 1);
        var endMonthLocal = startMonthLocal.AddMonths(1);
        var startMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startMonthLocal, Tz);
        var endMonthUtc = TimeZoneInfo.ConvertTimeToUtc(endMonthLocal, Tz);

        // --- Sayaçlar (ardışık) ---
        var totalEvents = await db.Events.CountAsync(ct);
        var thisMonth = await db.Events.Where(e => e.StartUtc >= startMonthUtc && e.StartUtc < endMonthUtc)
                                           .CountAsync(ct);
        var venuesCount = await db.Venues.CountAsync(ct);
        var newsCount = await db.News.CountAsync(ct);

        // --- Listeler (ardışık) ---
        var upcoming = await db.Events.AsNoTracking()
            .Include(e => e.Venue)
            .Where(e => e.IsPublished && e.EndUtc >= nowUtc)
            .OrderBy(e => e.StartUtc)
            .Take(5)
            .Select(e => new AdminEventRow(
                e.Id, e.Title,
                TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                e.Venue.Name, e.IsPublished))
            .ToListAsync(ct);

        var recentEvents = await db.Events.AsNoTracking()
            .Include(e => e.Venue)
            .OrderByDescending(e => e.StartUtc)
            .Take(5)
            .Select(e => new AdminEventRow(
                e.Id, e.Title,
                TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                e.Venue.Name, e.IsPublished))
            .ToListAsync(ct);

        var recentNews = await db.News.AsNoTracking()
            .OrderByDescending(n => n.PublishedAtUtc)
            .Take(5)
            .Select(n => new AdminNewsRow(
                n.Id, n.Title,
                TimeZoneInfo.ConvertTimeFromUtc(n.PublishedAtUtc, Tz)))
            .ToListAsync(ct);

        var venues = await db.Venues.AsNoTracking()
            .OrderBy(v => v.Name)
            .Take(5)
            .Select(v => new AdminVenueRow(v.Id, v.Name))
            .ToListAsync(ct);

        var counters = new AdminCounters(totalEvents, thisMonth, venuesCount, newsCount);

        return new AdminDashboardVm(counters, upcoming, recentEvents, recentNews, venues);
    }
}
