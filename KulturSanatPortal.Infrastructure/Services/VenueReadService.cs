// Infrastructure/Services/VenueReadService.cs
using KulturSanatPortal.Application.Venues;
using KulturSanatPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Infrastructure.Services;

public class VenueReadService(AppDbContext db) : IVenueReadService
{
    public async Task<IReadOnlyList<VenueDto>> ListAsync(CancellationToken ct)
        => await db.Venues.AsNoTracking()
            .OrderBy(v => v.Name)
            .Select(v => new VenueDto
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
                ImagePath = v.ImagePath
            })
            .ToListAsync(ct);

    public async Task<VenueDto?> GetBySlugAsync(string slug, CancellationToken ct)
        => await db.Venues.AsNoTracking()
            .Where(v => v.Slug == slug)
            .Select(v => new VenueDto
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
                ImagePath = v.ImagePath
            })
            .FirstOrDefaultAsync(ct);
}
