using System.ComponentModel.DataAnnotations;
namespace KulturSanatPortal.Web.Areas.Admin.Models
{
    public class EventFormVM
    {
        public int? Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = "";

        [StringLength(180)]
        public string? Slug { get; set; }

        [StringLength(280)]
        public string? Summary { get; set; }

        public string? DescriptionHtml { get; set; }

        [Required] public DateTime StartLocal { get; set; } = DateTime.Today.AddHours(20);
        [Required] public DateTime EndLocal { get; set; } = DateTime.Today.AddHours(22);

        [Required] public int VenueId { get; set; }
        [Required] public int CategoryId { get; set; }

        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        [Url] public string? TicketUrl { get; set; }
        public string? PriceInfo { get; set; }

        // NEW
        public string? ExistingHero { get; set; }
    }
}