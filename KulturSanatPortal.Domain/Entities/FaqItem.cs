namespace KulturSanatPortal.Domain.Entities;

public class FaqItem
{
    public int Id { get; set; }
    public string Question { get; set; } = "";
    public string AnswerHtml { get; set; } = "";
    public int SortOrder { get; set; }
}
