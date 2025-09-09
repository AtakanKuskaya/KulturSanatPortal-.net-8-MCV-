namespace KulturSanatPortal.Application.Categories;

public interface ICategoryReadService
{
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct);
}
