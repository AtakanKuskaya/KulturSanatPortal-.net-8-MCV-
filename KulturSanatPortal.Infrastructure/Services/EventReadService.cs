// Infrastructure/Services/EventReadService.cs
using KulturSanatPortal.Application.Common;
using KulturSanatPortal.Application.Events;
using KulturSanatPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Infrastructure.Services;

public class EventReadService(AppDbContext db) : IEventReadService
{
    private static TimeZoneInfo Tz => TimeZoneInfo.GetSystemTimeZones().Any(z => z.Id == "Turkey Standard Time")
        ? TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
        : TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public async Task<PagedResult<EventListItemDto>> ListUpcomingAsync(int? categoryId, int? venueId, int? year, int? month, int page, int pageSize, CancellationToken ct)
    {
        var q = db.Events.AsNoTracking()
            .Include(e => e.Venue).Include(e => e.Category)
            .Where(e => e.IsPublished && e.EndUtc >= DateTime.UtcNow);

        if (categoryId is not null) q = q.Where(e => e.CategoryId == categoryId);
        if (venueId is not null) q = q.Where(e => e.VenueId == venueId);
        if (year is not null && month is not null)
        {
            var startLocal = new DateTime(year.Value, month.Value, 1);
            var endLocal = startLocal.AddMonths(1);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, Tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, Tz);
            q = q.Where(e => e.StartUtc >= startUtc && e.StartUtc < endUtc);
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(e => e.StartUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new EventListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                VenueName = e.Venue.Name,
                CategoryName = e.Category.Name,
                Summary = e.Summary,
                HeroImagePath = e.HeroImagePath
            }).ToListAsync(ct);

        return new PagedResult<EventListItemDto>(items, total, page, pageSize);
    }

    public async Task<PagedResult<EventListItemDto>> ListPastAsync(int page, int pageSize, CancellationToken ct)
    {
        var q = db.Events.AsNoTracking()
            .Include(e => e.Venue).Include(e => e.Category)
            .Where(e => e.IsPublished && e.EndUtc < DateTime.UtcNow);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(e => e.StartUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new EventListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                VenueName = e.Venue.Name,
                CategoryName = e.Category.Name,
                Summary = e.Summary,
                HeroImagePath = e.HeroImagePath
            }).ToListAsync(ct);

        return new PagedResult<EventListItemDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<EventListItemDto>> ListForCalendarAsync(int year, int month, CancellationToken ct)
    {
        var startLocal = new DateTime(year, month, 1);
        var endLocal = startLocal.AddMonths(1);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, Tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, Tz);

        return await db.Events.AsNoTracking()
            .Include(e => e.Venue).Include(e => e.Category)
            .Where(e => e.IsPublished && e.StartUtc >= startUtc && e.StartUtc < endUtc)
            .OrderBy(e => e.StartUtc)
            .Select(e => new EventListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                VenueName = e.Venue.Name,
                CategoryName = e.Category.Name,
                Summary = e.Summary,
                HeroImagePath = e.HeroImagePath
            }).ToListAsync(ct);
    }

    public async Task<EventDetailsDto?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var e = await db.Events.AsNoTracking()
            .Include(x => x.Venue).Include(x => x.Category)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished, ct);

        return e is null ? null
            : new EventDetailsDto(
                id: e.Id, title: e.Title, slug: e.Slug, summary: e.Summary, descriptionHtml: e.DescriptionHtml,
                startLocal: TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                endLocal: TimeZoneInfo.ConvertTimeFromUtc(e.EndUtc, Tz),
                venueName: e.Venue.Name, categoryName: e.Category.Name,
                ticketUrl: e.TicketUrl, priceInfo: e.PriceInfo,
                heroImagePath: e.HeroImagePath,
                gallery: e.Images.OrderBy(i => i.SortOrder).Select(i => i.ImagePath).ToList()
            );
    }

    public async Task<IReadOnlyList<EventListItemDto>> ListFeaturedAsync(int take, CancellationToken ct)
        => await db.Events.AsNoTracking()
            .Include(e => e.Venue).Include(e => e.Category)
            .Where(e => e.IsPublished && e.IsFeatured && e.EndUtc >= DateTime.UtcNow)
            .OrderBy(e => e.StartUtc).Take(take)
            .Select(e => new EventListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                VenueName = e.Venue.Name,
                CategoryName = e.Category.Name,
                Summary = e.Summary,
                HeroImagePath = e.HeroImagePath
            }).ToListAsync(ct);

    public async Task<IReadOnlyList<EventListItemDto>> ListUpcomingByCategoryAsync(int categoryId, int take, CancellationToken ct)
        => await db.Events.AsNoTracking()
            .Include(e => e.Venue).Include(e => e.Category)
            .Where(e => e.IsPublished && e.EndUtc >= DateTime.UtcNow && e.CategoryId == categoryId)
            .OrderBy(e => e.StartUtc).Take(take)
            .Select(e => new EventListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                Slug = e.Slug,
                StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
                VenueName = e.Venue.Name,
                CategoryName = e.Category.Name,
                Summary = e.Summary,
                HeroImagePath = e.HeroImagePath
            }).ToListAsync(ct);
}
