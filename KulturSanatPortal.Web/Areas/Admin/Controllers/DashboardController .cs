using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KulturSanatPortal.Application.Dashboard;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize]
public class DashboardController(IDashboardReadService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await dashboard.GetAsync(ct));
}
