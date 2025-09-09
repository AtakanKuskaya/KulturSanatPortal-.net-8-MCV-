// KulturSanatPortal.Web/Areas/Admin/Controllers/EventsController.cs
using System.Linq;
using KulturSanatPortal.Domain.Entities;
using KulturSanatPortal.Infrastructure.Data;
using KulturSanatPortal.Web.Areas.Admin.Controllers.Models;
using KulturSanatPortal.Web.Areas.Admin.Controllers.Utils;
using KulturSanatPortal.Web.Areas.Admin.Models;
using KulturSanatPortal.Web.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize] // gerekirse: [Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
public class EventsController : Controller
{
    private readonly AppDbContext db;
    private readonly IFileStorage files;

    public EventsController(AppDbContext db, IFileStorage files)
    {
        this.db = db;
        this.files = files;
    }

    // Windows + Linux için TZ: bir kez çöz, cache'le
    private static readonly TimeZoneInfo Tz = ResolveTurkeyTz();
    private static TimeZoneInfo ResolveTurkeyTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
    }

    // Yükleme kontrolleri
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "image/avif" };
    private const long MaxImageSizeBytes = 8 * 1024 * 1024; // 8 MB

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 20;

        var q = db.Events.AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .OrderByDescending(e => e.StartUtc);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await FillLookups();
        return View(new EventFormVM());
    }

    [HttpPost]
    [RequestSizeLimit(32_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32_000_000)]
    public async Task<IActionResult> Create(EventFormVM m, IFormFile? hero, IFormFile[]? gallery)
    {
        await ValidateUploads(hero, gallery);

        if (m.EndLocal < m.StartLocal)
            ModelState.AddModelError(nameof(m.EndLocal), "Bitiş tarihi başlangıçtan önce olamaz.");

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Title) : Slugify.From(m.Slug!);
        if (await db.Events.AnyAsync(x => x.Slug == slug))
            ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı.");

        if (!ModelState.IsValid)
        {
            await FillLookups();
            return View(m);
        }

        // Dosya yollarını takip et (hata halinde temizlemek için)
        var savedFiles = new List<string>();

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            string? heroPath = null;
            if (hero is not null)
            {
                try
                {
                    heroPath = await files.SaveAsync(hero, "events");
                    savedFiles.Add(heroPath);
                }
                catch
                {
                    ModelState.AddModelError("hero", "Kapak görseli kaydedilemedi.");
                    await FillLookups();
                    return View(m);
                }
            }

            var e = new Event
            {
                Title = m.Title,
                Slug = slug,
                Summary = m.Summary,
                DescriptionHtml = m.DescriptionHtml,
                StartUtc = TimeZoneInfo.ConvertTimeToUtc(m.StartLocal, Tz),
                EndUtc = TimeZoneInfo.ConvertTimeToUtc(m.EndLocal, Tz),
                VenueId = m.VenueId,
                CategoryId = m.CategoryId,
                IsPublished = m.IsPublished,
                IsFeatured = m.IsFeatured,
                TicketUrl = m.TicketUrl,
                PriceInfo = m.PriceInfo,
                HeroImagePath = heroPath
            };
            db.Events.Add(e);
            await db.SaveChangesAsync();

            // Galeri
            if (gallery is not null && gallery.Length > 0)
            {
                var order = 0;
                foreach (var f in gallery.Where(g => g != null && g.Length > 0))
                {
                    var path = await files.SaveAsync(f!, "events");
                    savedFiles.Add(path);
                    db.EventImages.Add(new EventImage { EventId = e.Id, ImagePath = path, SortOrder = order++ });
                }
                await db.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await tx.RollbackAsync();
            // Dosyaları geri al
            foreach (var p in savedFiles)
            {
                try { await files.DeleteAsync(p); } catch { /* yut */ }
            }
            ModelState.AddModelError("", "Etkinlik kaydedilirken bir hata oluştu.");
            await FillLookups();
            return View(m);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Events.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();

        await FillLookups();

        var m = new EventFormVM
        {
            Id = e.Id,
            Title = e.Title,
            Slug = e.Slug,
            Summary = e.Summary,
            DescriptionHtml = e.DescriptionHtml,
            StartLocal = TimeZoneInfo.ConvertTimeFromUtc(e.StartUtc, Tz),
            EndLocal = TimeZoneInfo.ConvertTimeFromUtc(e.EndUtc, Tz),
            VenueId = e.VenueId,
            CategoryId = e.CategoryId,
            IsPublished = e.IsPublished,
            IsFeatured = e.IsFeatured,
            TicketUrl = e.TicketUrl,
            PriceInfo = e.PriceInfo,
            ExistingHero = e.HeroImagePath
        };
        ViewBag.Gallery = e.Images.OrderBy(x => x.SortOrder).ToList();
        return View(m);
    }

    [HttpPost]
    [RequestSizeLimit(32_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32_000_000)]
    public async Task<IActionResult> Edit(int id, EventFormVM m, IFormFile? hero, bool deleteHero = false, IFormFile[]? gallery = null)
    {
        await ValidateUploads(hero, gallery);

        if (m.EndLocal < m.StartLocal)
            ModelState.AddModelError(nameof(m.EndLocal), "Bitiş tarihi başlangıçtan önce olamaz.");

        var e = await db.Events.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Title) : Slugify.From(m.Slug!);
        if (await db.Events.AnyAsync(x => x.Slug == slug && x.Id != id))
            ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı.");

        if (!ModelState.IsValid)
        {
            await FillLookups();
            ViewBag.Gallery = e.Images.OrderBy(x => x.SortOrder).ToList();
            return View(m);
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            e.Title = m.Title;
            e.Slug = slug;
            e.Summary = m.Summary;
            e.DescriptionHtml = m.DescriptionHtml;
            e.StartUtc = TimeZoneInfo.ConvertTimeToUtc(m.StartLocal, Tz);
            e.EndUtc = TimeZoneInfo.ConvertTimeToUtc(m.EndLocal, Tz);
            e.VenueId = m.VenueId;
            e.CategoryId = m.CategoryId;
            e.IsPublished = m.IsPublished;
            e.IsFeatured = m.IsFeatured;
            e.TicketUrl = m.TicketUrl;
            e.PriceInfo = m.PriceInfo;

            // Kapak işlemleri
            if (deleteHero && !string.IsNullOrWhiteSpace(e.HeroImagePath))
            {
                try { await files.DeleteAsync(e.HeroImagePath); } catch { }
                e.HeroImagePath = null;
            }

            if (hero is not null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(e.HeroImagePath))
                        try { await files.DeleteAsync(e.HeroImagePath); } catch { }

                    e.HeroImagePath = await files.SaveAsync(hero, "events");
                }
                catch
                {
                    ModelState.AddModelError("hero", "Kapak görseli kaydedilemedi.");
                    await FillLookups();
                    ViewBag.Gallery = e.Images.OrderBy(x => x.SortOrder).ToList();
                    return View(m);
                }
            }

            // Galeri ekle
            if (gallery is not null && gallery.Length > 0)
            {
                var startOrder = (e.Images.Count == 0) ? 0 : e.Images.Max(i => i.SortOrder) + 1;
                foreach (var f in gallery.Where(g => g != null && g.Length > 0))
                {
                    var path = await files.SaveAsync(f!, "events");
                    db.EventImages.Add(new EventImage { EventId = e.Id, ImagePath = path, SortOrder = startOrder++ });
                }
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }
        catch
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", "Değişiklikler kaydedilirken bir hata oluştu.");
            await FillLookups();
            ViewBag.Gallery = (await db.EventImages.Where(x => x.EventId == id).OrderBy(x => x.SortOrder).ToListAsync());
            return View(m);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var img = await db.EventImages.FirstOrDefaultAsync(x => x.Id == imageId && x.EventId == id);
        if (img is not null)
        {
            try { await files.DeleteAsync(img.ImagePath); } catch { /* yut */ }
            db.EventImages.Remove(img);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Edit), new { id });
    }

    // Etkinliği tamamen sil (kapak + galeri dosyalarıyla)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Events.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(e.HeroImagePath))
                try { await files.DeleteAsync(e.HeroImagePath); } catch { }

            foreach (var gi in e.Images)
                try { await files.DeleteAsync(gi.ImagePath); } catch { }

            db.EventImages.RemoveRange(e.Images);
            db.Events.Remove(e);

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", "Etkinlik silinirken bir hata oluştu.");
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    // Galeri sıralama (drag&drop sonrası)
    [HttpPost]
    public async Task<IActionResult> ReorderGallery(int id, [FromBody] int[] imageIds)
    {
        if (imageIds is null || imageIds.Length == 0) return BadRequest();

        var imgs = await db.EventImages.Where(x => x.EventId == id)
            .ToDictionaryAsync(x => x.Id);

        var order = 0;
        foreach (var imgId in imageIds)
            if (imgs.TryGetValue(imgId, out var img))
                img.SortOrder = order++;

        await db.SaveChangesAsync();
        return Ok();
    }

    // --- Yardımcılar ---
    private async Task FillLookups()
    {
        ViewBag.Categories = await db.EventCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        ViewBag.Venues = await db.Venues.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    }

    private Task ValidateUploads(IFormFile? hero, IFormFile[]? gallery)
    {
        void Check(IFormFile f, string field)
        {
            if (f.Length > MaxImageSizeBytes)
                ModelState.AddModelError(field, "Görsel 8 MB’ı geçemez.");
            if (!AllowedImageTypes.Contains(f.ContentType))
                ModelState.AddModelError(field, "Dosya türü desteklenmiyor. (jpg, png, webp, avif)");
        }

        if (hero is not null) Check(hero, "hero");
        if (gallery is not null)
            for (int i = 0; i < gallery.Length; i++)
                if (gallery[i] is not null) Check(gallery[i], $"gallery[{i}]");

        return Task.CompletedTask;
    }
}
