using System.Security.Claims;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Users;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public List<AdminUser> Users { get; private set; } = [];

    public int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public async Task OnGetAsync()
    {
        Users = await dbContext.AdminUsers
            .OrderByDescending(user => user.Role == AdminUserRoles.SuperAdmin)
            .ThenBy(user => user.DisplayName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (id == CurrentUserId)
        {
            return RedirectToPage();
        }

        var user = await dbContext.AdminUsers.FindAsync(id);
        if (user is not null)
        {
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
