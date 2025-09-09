using KulturSanatPortal.Application.News;
using Microsoft.AspNetCore.Mvc;

public class NewsController(INewsReadService svc) : Controller
{
    [Route("haberler")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
        => View(await svc.ListAsync(page, 12, ct));

    [Route("haber/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct = default)
    {
        var d = await svc.GetBySlugAsync(slug, ct);
        return d is null ? NotFound() : View(d);
    }
}
