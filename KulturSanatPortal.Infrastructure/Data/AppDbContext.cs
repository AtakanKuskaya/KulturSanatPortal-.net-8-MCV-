using KulturSanatPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KulturSanatPortal.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<News> News => Set<News>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();

    // NEW
    public DbSet<EventImage> EventImages => Set<EventImage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Venue>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<EventCategory>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Event>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<News>().HasIndex(x => x.Slug).IsUnique();

        b.Entity<Event>()
            .HasOne(e => e.Venue).WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Event>()
            .HasOne(e => e.Category).WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // NEW: EventImage
        b.Entity<EventImage>()
            .HasOne(x => x.Event).WithMany(e => e.Images)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<EventImage>().HasIndex(x => new { x.EventId, x.SortOrder });
    }
}

