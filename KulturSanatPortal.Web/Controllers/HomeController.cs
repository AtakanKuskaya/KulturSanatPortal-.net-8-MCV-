using System.Globalization;
using KulturSanatPortal.Application.Categories;
using KulturSanatPortal.Application.Events;
using KulturSanatPortal.Application.News;
using KulturSanatPortal.Web.Models.Home; // HomePageVm, MiniCalendarVm, MiniDay, EventCard, CategoryBlock, NewsCard
using Microsoft.AspNetCore.Mvc;

namespace KulturSanatPortal.Web.Controllers;

public class HomeController(
    IEventReadService events,
    ICategoryReadService categories,
    INewsReadService news) : Controller
{
    // TR saat dilimi sadece "bugün" gibi yerel hesaplar için gerektiğinde:
    private static TimeZoneInfo Tz =>
        TimeZoneInfo.GetSystemTimeZones().Any(z => z.Id == "Turkey Standard Time")
            ? TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    [HttpGet]
    public async Task<IActionResult> Index(int? y, int? m, CancellationToken ct = default)
    {
        // 1) Ay bilgisi + gezinti (prev/next)
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tz);
        var year = y ?? nowLocal.Year;
        var month = m ?? nowLocal.Month;

        var monthStartLocal = new DateTime(year, month, 1);
        var prev = monthStartLocal.AddMonths(-1);
        var next = monthStartLocal.AddMonths(1);
        ViewBag.PrevY = prev.Year; ViewBag.PrevM = prev.Month;
        ViewBag.NextY = next.Year; ViewBag.NextM = next.Month;

        // 2) Mini takvim (42 hücre, pazartesi başlangıç)
        var gridStartLocal = monthStartLocal;
        while (gridStartLocal.DayOfWeek != DayOfWeek.Monday) gridStartLocal = gridStartLocal.AddDays(-1);
        var gridEndLocal = gridStartLocal.AddDays(42);

        var monthItems = await events.ListForCalendarAsync(year, month, ct);
        var counts = monthItems
            .GroupBy(e => e.StartLocal.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var days = Enumerable.Range(0, 42).Select(i =>
        {
            var d = gridStartLocal.AddDays(i);
            return new MiniDay(
                d,
                d.Month == month,
                d.Date == nowLocal.Date,
                counts.TryGetValue(d.Date, out var c) ? c : 0
            );
        }).ToList();



        var mini = new MiniCalendarVm { Year = year, Month = month, Days = days };

        // 3) Öne çıkan etkinlikler
        var featured = await events.ListFeaturedAsync(6, ct);
        var featuredCards = featured.Select(e => new EventCard(
            Id: e.Id, Title: e.Title, Slug: e.Slug,
            StartLocal: e.StartLocal, VenueName: e.VenueName, CategoryName: e.CategoryName,
            Summary: e.Summary, HeroImagePath: e.HeroImagePath
        )).ToList();

        // 4) Kategori sekmeleri (ilk 5 kategori; her birinden 6 yaklaşan)
        var catDtos = (await categories.ListAsync(ct)).Take(5).ToList();
        var catBlocks = new List<CategoryBlock>();
        foreach (var c in catDtos)
        {
            var list = await events.ListUpcomingByCategoryAsync(c.Id, 6, ct);
            if (list.Count > 0)
            {
                var evts = list.Select(e => new EventCard(
                    e.Id, e.Title, e.Slug, e.StartLocal, e.VenueName, e.CategoryName, e.Summary, e.HeroImagePath
                )).ToList();

                catBlocks.Add(new CategoryBlock(c.Id, c.Name, c.Slug, evts));
            }
        }

        // 5) Haberler (sayfa:1, boyut:6; kapak yoksa view placeholder ile çözüyor)
        // 5) Haberler
        var latestPaged = await news.ListAsync(1, 6, ct);
        var newsCards = latestPaged.Items.Select(n => new NewsCard(
            n.Id,
            n.Title,
            n.Slug,
            n.PublishedLocal,
            n.Summary,
            FixPath(n.HeroImagePath)    // ← gerçek kapak yolu
        )).ToList();

        // 6) VM
        var vm = new HomePageVm
        {
            Featured = featuredCards,
            Categories = catBlocks,
            News = newsCards,
            Calendar = mini
        };

        return View(vm);
    }
    static string? FixPath(string? p)
    {
        if (string.IsNullOrWhiteSpace(p)) return null;
        var s = p.Replace("\\", "/").TrimStart('~');
        if (!s.StartsWith("/")) s = "/" + s;
        return s;
    }
}
