using KulturSanatPortal.Application.Common;
using KulturSanatPortal.Application.News;
using KulturSanatPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Infrastructure.Services
{
    public class NewsReadService : INewsReadService
    {
        private readonly AppDbContext db;
        public NewsReadService(AppDbContext db) => this.db = db;

        private static TimeZoneInfo Tz => ResolveTurkeyTz();
        private static TimeZoneInfo ResolveTurkeyTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        }

        public async Task<PagedResult<NewsListItemDto>> ListAsync(int page, int pageSize, CancellationToken ct)
        {
            var q = db.News.AsNoTracking().Where(n => n.IsPublished);

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(n => n.PublishedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewsListItemDto(
                    n.Id,
                    n.Title,
                    n.Slug,
                    TimeZoneInfo.ConvertTimeFromUtc(n.PublishedAtUtc, Tz),
                    n.Summary,
                    n.HeroImagePath
                ))
                .ToListAsync(ct);

            return new PagedResult<NewsListItemDto>(items, total, page, pageSize);
        }

        public async Task<IReadOnlyList<NewsListItemDto>> ListLatestAsync(int take, CancellationToken ct)
        {
            return await db.News.AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.PublishedAtUtc)
                .Take(take)
                .Select(n => new NewsListItemDto(
                    n.Id,
                    n.Title,
                    n.Slug,
                    TimeZoneInfo.ConvertTimeFromUtc(n.PublishedAtUtc, Tz),
                    n.Summary,
                    n.HeroImagePath
                ))
                .ToListAsync(ct);
        }

        public async Task<NewsDetailsDto?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            var n = await db.News.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished, ct);

            if (n is null) return null;

            // İçerik alanı DB’de ContentHtml veya DescriptionHtml olabilir.
            // Derleme hatası vermemesi için reflection ile okunuyor:
            string? contentHtml =
                n.GetType().GetProperty("ContentHtml")?.GetValue(n) as string
                ?? n.GetType().GetProperty("DescriptionHtml")?.GetValue(n) as string;

            return new NewsDetailsDto(
                n.Id,
                n.Title,
                n.Slug,
                TimeZoneInfo.ConvertTimeFromUtc(n.PublishedAtUtc, Tz),
                n.Summary,
                contentHtml,
                n.HeroImagePath
            );
        }
    }
}
