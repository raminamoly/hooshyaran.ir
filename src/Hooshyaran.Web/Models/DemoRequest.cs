namespace Hooshyaran.Web.Models;

public class DemoRequest
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NeedArea { get; set; } = string.Empty;

    public string PreferredTime { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = DemoRequestStatuses.New;

    public string AdminNotes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
