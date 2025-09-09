using KulturSanatPortal.Application.Categories;
using KulturSanatPortal.Application.Dashboard;
using KulturSanatPortal.Application.Events;
using KulturSanatPortal.Application.News;
using KulturSanatPortal.Application.Venues;
using KulturSanatPortal.Infrastructure.Data;
using KulturSanatPortal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KulturSanatPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            var server = cfg["ConnectionConfig:ServerName"] ?? "localhost";
            var uid = cfg["ConnectionConfig:UserId"] ?? "aybat";
            var pwd = cfg["ConnectionConfig:Password"] ?? "aybat";
            var cs = cfg.GetConnectionString("Default")!
                .Replace("{ServerName}", server)
                .Replace("{UserId}", uid)
                .Replace("{Password}", pwd);
            opt.UseSqlServer(cs);
        });

        services.AddScoped<IEventReadService, EventReadService>();
        services.AddScoped<ICategoryReadService, CategoryReadService>();
        services.AddScoped<INewsReadService, NewsReadService>();
        services.AddScoped<IVenueReadService, VenueReadService>();
        services.AddScoped<IDashboardReadService, DashboardReadService>();

        return services;
    }
}
