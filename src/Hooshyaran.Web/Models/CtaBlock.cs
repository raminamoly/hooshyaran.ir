namespace Hooshyaran.Web.Models;

public class CtaBlock
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ButtonText { get; set; } = string.Empty;

    public string ButtonUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
