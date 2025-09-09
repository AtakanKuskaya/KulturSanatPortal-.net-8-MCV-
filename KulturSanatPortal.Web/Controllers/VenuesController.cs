using KulturSanatPortal.Application.Venues;
using Microsoft.AspNetCore.Mvc;

public class VenuesController(IVenueReadService svc) : Controller
{
    [Route("salonlar")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await svc.ListAsync(ct));

    [Route("salon-detay/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var v = await svc.GetBySlugAsync(slug, ct);
        return v is null ? NotFound() : View(v);
    }
}
