using System.ComponentModel.DataAnnotations;

namespace KulturSanatPortal.Web.Areas.Admin.Models
{

    public class NewsFormVM
    {
        public int? Id { get; set; }

        [Required, StringLength(180)]
        public string Title { get; set; } = "";

        [StringLength(180)]
        public string? Slug { get; set; }

        [StringLength(300)]
        public string? Summary { get; set; }

        public string? BodyHtml { get; set; }

        [Required] public DateTime PublishedLocal { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;

        public string? ExistingHero { get; set; }
    }
}
