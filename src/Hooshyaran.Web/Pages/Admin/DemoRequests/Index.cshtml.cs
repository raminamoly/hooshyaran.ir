using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.DemoRequests;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public List<DemoRequest> Requests { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Requests = await dbContext.DemoRequests
            .ToListAsync();

        Requests = Requests
            .OrderByDescending(request => request.CreatedAt)
            .ToList();
    }
}
