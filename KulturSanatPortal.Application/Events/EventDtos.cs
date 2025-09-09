// Application/Events/EventDtos.cs
using System;
using System.Collections.Generic;

namespace KulturSanatPortal.Application.Events
{
    // Liste kartı için DTO
    public record EventListItemDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string Slug { get; init; } = "";
        public DateTime StartLocal { get; init; }
        public string VenueName { get; init; } = "";
        public string CategoryName { get; init; } = "";
        public string? Summary { get; init; }
        public string? HeroImagePath { get; init; }  // Yeni (liste kapak görseli)

        public EventListItemDto() { }

        // Geriye dönük uyumluluk (eski 7 parametreli kurucu)
        public EventListItemDto(
            int id, string title, string slug,
            DateTime startLocal, string venueName, string categoryName, string? summary)
        {
            Id = id; Title = title; Slug = slug;
            StartLocal = startLocal; VenueName = venueName; CategoryName = categoryName;
            Summary = summary; HeroImagePath = null;
        }
    }

    // Detay sayfası için DTO
    public record EventDetailsDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string Slug { get; init; } = "";
        public string? Summary { get; init; }
        public string? DescriptionHtml { get; init; }
        public DateTime StartLocal { get; init; }
        public DateTime EndLocal { get; init; }
        public string VenueName { get; init; } = "";
        public string CategoryName { get; init; } = "";
        public string? TicketUrl { get; init; }
        public string? PriceInfo { get; init; }
        public string? HeroImagePath { get; init; }                 // Yeni
        public IReadOnlyList<string> Gallery { get; init; } = Array.Empty<string>(); // Yeni

        public EventDetailsDto() { }

        // Geriye dönük uyumluluk (eski kurucu: kapak ve galeri yok)
        public EventDetailsDto(
            int id, string title, string slug, string? summary, string? descriptionHtml,
            DateTime startLocal, DateTime endLocal, string venueName, string categoryName,
            string? ticketUrl, string? priceInfo)
        {
            Id = id; Title = title; Slug = slug; Summary = summary; DescriptionHtml = descriptionHtml;
            StartLocal = startLocal; EndLocal = endLocal; VenueName = venueName; CategoryName = categoryName;
            TicketUrl = ticketUrl; PriceInfo = priceInfo;
            HeroImagePath = null; Gallery = Array.Empty<string>();
        }

        // İsteğe bağlı: Tüm alanları alan yeni kurucu (kullanmak istersen)
        public EventDetailsDto(
            int id, string title, string slug, string? summary, string? descriptionHtml,
            DateTime startLocal, DateTime endLocal, string venueName, string categoryName,
            string? ticketUrl, string? priceInfo, string? heroImagePath, IReadOnlyList<string> gallery)
        {
            Id = id; Title = title; Slug = slug; Summary = summary; DescriptionHtml = descriptionHtml;
            StartLocal = startLocal; EndLocal = endLocal; VenueName = venueName; CategoryName = categoryName;
            TicketUrl = ticketUrl; PriceInfo = priceInfo; HeroImagePath = heroImagePath;
            Gallery = gallery ?? Array.Empty<string>();
        }
    }
}
