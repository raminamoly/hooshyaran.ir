using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hooshyaran.Web.Pages.Admin.DemoRequests;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class EditModel(HooshyaranDbContext dbContext) : PageModel
{
    [BindProperty]
    public DemoRequestInput Input { get; set; } = new();

    public DemoRequest RequestItem { get; private set; } = new();

    public List<SelectListItem> StatusOptions { get; } =
    [
        new(DemoRequestStatuses.ToPersian(DemoRequestStatuses.New), DemoRequestStatuses.New),
        new(DemoRequestStatuses.ToPersian(DemoRequestStatuses.Reviewed), DemoRequestStatuses.Reviewed),
        new(DemoRequestStatuses.ToPersian(DemoRequestStatuses.Contacted), DemoRequestStatuses.Contacted),
        new(DemoRequestStatuses.ToPersian(DemoRequestStatuses.Closed), DemoRequestStatuses.Closed)
    ];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var demoRequest = await dbContext.DemoRequests.FindAsync(id);
        if (demoRequest is null)
        {
            return NotFound();
        }

        RequestItem = demoRequest;
        Input = new DemoRequestInput
        {
            Id = demoRequest.Id,
            Status = demoRequest.Status,
            AdminNotes = demoRequest.AdminNotes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var demoRequest = await dbContext.DemoRequests.FindAsync(Input.Id);
        if (demoRequest is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            RequestItem = demoRequest;
            return Page();
        }

        demoRequest.Status = Input.Status;
        demoRequest.AdminNotes = Input.AdminNotes;
        demoRequest.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/DemoRequests/Index");
    }

    public class DemoRequestInput
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "وضعیت")]
        public string Status { get; set; } = DemoRequestStatuses.New;

        [Display(Name = "یادداشت داخلی")]
        public string AdminNotes { get; set; } = string.Empty;
    }
}
