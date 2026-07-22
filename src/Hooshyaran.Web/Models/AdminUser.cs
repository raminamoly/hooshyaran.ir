namespace Hooshyaran.Web.Models;

public class AdminUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = AdminUserRoles.Author;

    public bool IsActive { get; set; } = true;

    public bool ReceiveDemoRequestNotifications { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<BlogArticle> BlogArticles { get; set; } = [];
}
