
using System.ComponentModel.DataAnnotations;
namespace KulturSanatPortal.Web.Areas.Admin.Controllers.Models;
public class VenueFormVM
{
    public int? Id { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = "";

    [StringLength(180)]
    public string? Slug { get; set; }

    public int? Capacity { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? MapEmbedUrl { get; set; }
    public string? DescriptionHtml { get; set; }

    public string? ExistingImage { get; set; }
}
