using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KulturSanatPortal.Infrastructure.Data
{
    public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // appsettings.json'ı bul (komutu Web veya sln kökünden çalıştırmana göre güvenli)
            var basePath = Directory.GetCurrentDirectory();
            var webPath = Path.Combine(basePath, "..", "KulturSanatPortal.Web");

            if (!File.Exists(Path.Combine(basePath, "appsettings.json")) &&
                File.Exists(Path.Combine(webPath, "appsettings.json")))
            {
                basePath = webPath; // Web projesindeki appsettings'i kullan
            }

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var cfg = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // Connection string öncelik sırası: appsettings -> ENV -> fallback
            var cs = cfg.GetConnectionString("Default")
                  ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                  ?? "Server=.;Database=KulturSanatDb;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs)
                .Options;

            return new AppDbContext(options);
        }
    }
}
