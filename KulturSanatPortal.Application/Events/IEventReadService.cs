using KulturSanatPortal.Application.Common;

namespace KulturSanatPortal.Application.Events;

public interface IEventReadService
{
    Task<PagedResult<EventListItemDto>> ListUpcomingAsync(
        int? categoryId, int? venueId, int? year, int? month, int page, int pageSize, CancellationToken ct);

    Task<PagedResult<EventListItemDto>> ListPastAsync(int page, int pageSize, CancellationToken ct);

    Task<IReadOnlyList<EventListItemDto>> ListForCalendarAsync(int year, int month, CancellationToken ct);

    Task<EventDetailsDto?> GetBySlugAsync(string slug, CancellationToken ct);

    // NEW: Home için öne çıkanlar
    Task<IReadOnlyList<EventListItemDto>> ListFeaturedAsync(int take, CancellationToken ct);

    // NEW: Home kategori blokları için
    Task<IReadOnlyList<EventListItemDto>> ListUpcomingByCategoryAsync(int categoryId, int take, CancellationToken ct);
}
