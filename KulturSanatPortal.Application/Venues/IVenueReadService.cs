using KulturSanatPortal.Application.Venues;

namespace KulturSanatPortal.Application.Venues;

public interface IVenueReadService
{
    Task<IReadOnlyList<VenueDto>> ListAsync(CancellationToken ct);
    Task<VenueDto?> GetBySlugAsync(string slug, CancellationToken ct);
}
