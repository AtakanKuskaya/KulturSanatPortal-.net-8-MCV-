// KulturSanatPortal.Web/Controllers/EventsController.cs
using System.Globalization;
using System.Linq;
using KulturSanatPortal.Application.Events;
using KulturSanatPortal.Web.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace KulturSanatPortal.Web.Controllers;

public class EventsController(IEventReadService svc) : Controller
{
    private const int PageSize = 12;
    private static readonly CultureInfo Tr = new("tr-TR");

    // /etkinlikler  ve /events
    [HttpGet]
    [Route("etkinlikler", Name = "EventsIndex")]
    [Route("events")]
    public async Task<IActionResult> Index(
        int? categoryId, int? venueId, int? y, int? m, int page = 1, CancellationToken ct = default)
        => View(await svc.ListUpcomingAsync(categoryId, venueId, y, m, page, PageSize, ct));

    // /etkinlikler/gecmis
    [HttpGet("etkinlikler/gecmis")]
    public async Task<IActionResult> Past(int page = 1, CancellationToken ct = default)
        => View(await svc.ListPastAsync(page, PageSize, ct));

    // /etkinlikler/takvim
    [HttpGet("etkinlikler/takvim")]
    public async Task<IActionResult> Calendar(int? y, int? m, CancellationToken ct = default)
    {
        var now = DateTime.Today;
        var year = y ?? now.Year;
        var month = m ?? now.Month;
        var items = await svc.ListForCalendarAsync(year, month, ct);
        ViewBag.Year = year; ViewBag.Month = month;
        return View(items);
    }

    // /etkinlik/{slug}
    [HttpGet("etkinlik/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct = default)
    {
        var d = await svc.GetBySlugAsync(slug, ct);
        return d is null ? NotFound() : View(d);
    }

    // /etkinlikler/gun/2025/9/8  (ayrıca /Events/Day de çalışsın)
    [HttpGet]
    [Route("etkinlikler/gun/{year:int}/{month:int}/{day:int}")]
    [Route("events/day/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Day(int year, int month, int day, CancellationToken ct = default)
    {
        // Geçersiz tarihleri kibarca takvim sayfasına yönlendir
        DateTime target;
        try { target = new DateTime(year, month, day); }
        catch { return RedirectToAction(nameof(Calendar), new { y = year, m = month }); }

        var monthItems = await svc.ListForCalendarAsync(year, month, ct);

        var list = monthItems
            .Where(e => e.StartLocal.Date == target.Date)
            .Select(e => new EventCard(
                e.Id, e.Title, e.Slug,
                e.StartLocal, e.VenueName, e.CategoryName, e.Summary,
                e.HeroImagePath // artık kapak da geliyor
            ))
            .ToList();

        ViewBag.DateTitle = target.ToString("dd MMMM yyyy", Tr);
        return View(list);
    }
}
