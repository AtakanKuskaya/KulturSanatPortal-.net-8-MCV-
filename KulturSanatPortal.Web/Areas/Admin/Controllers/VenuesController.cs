using System.ComponentModel.DataAnnotations;
using KulturSanatPortal.Domain.Entities;
using KulturSanatPortal.Infrastructure.Data;
using KulturSanatPortal.Web.Areas.Admin.Controllers.Models;
using KulturSanatPortal.Web.Areas.Admin.Controllers.Utils;
using KulturSanatPortal.Web.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize]
public class VenuesController(AppDbContext db, IFileStorage files) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 20;
        var q = db.Venues.OrderBy(v => v.Name);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = pageSize;
        return View(items);
    }

    public IActionResult Create() => View(new VenueFormVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueFormVM m, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(m);

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Name) : Slugify.From(m.Slug!);
        if (await db.Venues.AnyAsync(x => x.Slug == slug))
        {
            ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı.");
            return View(m);
        }

        string? imagePath = null;
        if (image is not null) imagePath = await files.SaveAsync(image, "venues");

        var v = new Venue
        {
            Name = m.Name,
            Slug = slug,
            Capacity = m.Capacity,
            Address = m.Address,
            District = m.District,
            Phone = m.Phone,
            Email = m.Email,
            MapEmbedUrl = m.MapEmbedUrl,
            DescriptionHtml = m.DescriptionHtml,
            ImagePath = imagePath
        };
        db.Venues.Add(v);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var v = await db.Venues.FindAsync(id);
        if (v is null) return NotFound();
        var m = new VenueFormVM
        {
            Id = v.Id,
            Name = v.Name,
            Slug = v.Slug,
            Capacity = v.Capacity,
            Address = v.Address,
            District = v.District,
            Phone = v.Phone,
            Email = v.Email,
            MapEmbedUrl = v.MapEmbedUrl,
            DescriptionHtml = v.DescriptionHtml,
            ExistingImage = v.ImagePath
        };
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VenueFormVM m, IFormFile? image, bool deleteImage = false)
    {
        if (!ModelState.IsValid) return View(m);
        var v = await db.Venues.FirstOrDefaultAsync(x => x.Id == id);
        if (v is null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(m.Slug) ? Slugify.From(m.Name) : Slugify.From(m.Slug!);
        if (await db.Venues.AnyAsync(x => x.Slug == slug && x.Id != id))
        {
            ModelState.AddModelError(nameof(m.Slug), "Slug benzersiz olmalı.");
            return View(m);
        }

        v.Name = m.Name; v.Slug = slug; v.Capacity = m.Capacity;
        v.Address = m.Address; v.District = m.District; v.Phone = m.Phone; v.Email = m.Email;
        v.MapEmbedUrl = m.MapEmbedUrl; v.DescriptionHtml = m.DescriptionHtml;

        if (deleteImage && !string.IsNullOrWhiteSpace(v.ImagePath))
        {
            await files.DeleteAsync(v.ImagePath);
            v.ImagePath = null;
        }
        if (image is not null)
        {
            if (!string.IsNullOrWhiteSpace(v.ImagePath)) await files.DeleteAsync(v.ImagePath);
            v.ImagePath = await files.SaveAsync(image, "venues");
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var v = await db.Venues.FirstOrDefaultAsync(x => x.Id == id);
        return v is null ? NotFound() : View(v);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var v = await db.Venues.FindAsync(id);
        if (v is not null)
        {
            if (!string.IsNullOrWhiteSpace(v.ImagePath)) await files.DeleteAsync(v.ImagePath);
            db.Venues.Remove(v);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
