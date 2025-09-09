// Infrastructure/Services/CategoryReadService.cs
using KulturSanatPortal.Application.Categories;
using KulturSanatPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KulturSanatPortal.Infrastructure.Services;

public class CategoryReadService(AppDbContext db) : ICategoryReadService
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct)
        => await db.EventCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(ct);
}
