using System.ComponentModel.DataAnnotations;
using KulturSanatPortal.Domain.Entities;
using KulturSanatPortal.Infrastructure.Data;
using KulturSanatPortal.Web.Areas.Admin.Controllers.Utils;
using KulturSanatPortal.Web.Areas.Admin.Models;
using KulturSanatPortal.Web.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize]
public class NewsController(AppDbContext db, IFileStorage files) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 20;
        var q = db.News.OrderByDescending(n => n.PublishedAtUtc);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = pageSize;
        return View(items);
    }

    public IActionResult Create() => View(new NewsFormVM { PublishedLocal = DateTime.Now });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsFormVM m, IFormFile? hero)
    {
        if (!ModelState.IsValid) return View(m);

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Title) : Slugify.From(m.Slug!);
        if (await db.News.AnyAsync(x => x.Slug == slug))
        {
            ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı."); return View(m);
        }

        string? heroPath = null;
        if (hero is not null) heroPath = await files.SaveAsync(hero, "news");

        var tz = GetTz();
        var n = new News
        {
            Title = m.Title,
            Slug = slug,
            Summary = m.Summary,
            BodyHtml = m.BodyHtml ?? "",
            PublishedAtUtc = TimeZoneInfo.ConvertTimeToUtc(m.PublishedLocal, tz),
            IsPublished = m.IsPublished,
            HeroImagePath = heroPath
        };
        db.News.Add(n);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var n = await db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return NotFound();
        var tz = GetTz();
        var m = new NewsFormVM
        {
            Id = n.Id,
            Title = n.Title,
            Slug = n.Slug,
            Summary = n.Summary,
            BodyHtml = n.BodyHtml,
            PublishedLocal = TimeZoneInfo.ConvertTimeFromUtc(n.PublishedAtUtc, tz),
            IsPublished = n.IsPublished,
            ExistingHero = n.HeroImagePath
        };
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewsFormVM m, IFormFile? hero, bool deleteHero = false)
    {
        if (!ModelState.IsValid) return View(m);
        var n = await db.News.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Title) : Slugify.From(m.Slug!);
        if (await db.News.AnyAsync(x => x.Slug == slug && x.Id != id))
        { ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı."); return View(m); }

        n.Title = m.Title; n.Slug = slug; n.Summary = m.Summary; n.BodyHtml = m.BodyHtml ?? "";
        var tz = GetTz();
        n.PublishedAtUtc = TimeZoneInfo.ConvertTimeToUtc(m.PublishedLocal, tz);
        n.IsPublished = m.IsPublished;

        if (deleteHero && !string.IsNullOrWhiteSpace(n.HeroImagePath))
        { await files.DeleteAsync(n.HeroImagePath); n.HeroImagePath = null; }
        if (hero is not null)
        {
            if (!string.IsNullOrWhiteSpace(n.HeroImagePath)) await files.DeleteAsync(n.HeroImagePath);
            n.HeroImagePath = await files.SaveAsync(hero, "news");
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var n = await db.News.FirstOrDefaultAsync(x => x.Id == id);
        return n is null ? NotFound() : View(n);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var n = await db.News.FindAsync(id);
        if (n is not null)
        {
            if (!string.IsNullOrWhiteSpace(n.HeroImagePath)) await files.DeleteAsync(n.HeroImagePath);
            db.News.Remove(n);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static TimeZoneInfo GetTz() =>
        TimeZoneInfo.GetSystemTimeZones().Any(z => z.Id == "Turkey Standard Time")
            ? TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
}


