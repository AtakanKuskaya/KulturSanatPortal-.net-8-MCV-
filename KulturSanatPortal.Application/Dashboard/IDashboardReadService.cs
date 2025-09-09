namespace KulturSanatPortal.Application.Dashboard;

public interface IDashboardReadService
{
    Task<AdminDashboardVm> GetAsync(CancellationToken ct = default);
}
