using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class RequestDemoModel(
    HooshyaranDbContext dbContext,
    IDemoRequestNotificationService demoRequestNotificationService,
    ISiteVisitLogger visitLogger) : PageModel
{
    [BindProperty]
    public DemoInput Input { get; set; } = new();

    public bool Submitted { get; private set; }

    public StaticPage? PageContent { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadContentAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadContentAsync();
            return Page();
        }

        var demoRequest = new DemoRequest
        {
            FullName = Input.FullName.Trim(),
            OrganizationName = Input.OrganizationName.Trim(),
            JobTitle = (Input.JobTitle ?? string.Empty).Trim(),
            PhoneNumber = Input.PhoneNumber.Trim(),
            Email = (Input.Email ?? string.Empty).Trim(),
            NeedArea = (Input.NeedArea ?? string.Empty).Trim(),
            PreferredTime = (Input.PreferredTime ?? string.Empty).Trim(),
            Notes = (Input.Notes ?? string.Empty).Trim(),
            Status = DemoRequestStatuses.New,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DemoRequests.Add(demoRequest);
        await dbContext.SaveChangesAsync();
        await visitLogger.LogAsync(HttpContext, SiteVisitEventTypes.DemoSubmit, "ثبت فرم درخواست دمو");
        await demoRequestNotificationService.NotifyAsync(demoRequest);

        ModelState.Clear();
        Input = new DemoInput();
        Submitted = true;
        await LoadContentAsync();
        return Page();
    }

    private async Task LoadContentAsync()
    {
        PageContent = await dbContext.StaticPages
            .AsNoTracking()
            .SingleOrDefaultAsync(page => page.Key == "request-demo" && page.IsPublished);
    }

    public class DemoInput
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام سازمان یا شرکت الزامی است.")]
        [Display(Name = "نام سازمان یا شرکت")]
        public string OrganizationName { get; set; } = string.Empty;

        [Display(Name = "سمت سازمانی")]
        public string? JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل کاری")]
        public string? Email { get; set; } = string.Empty;

        [Display(Name = "موضوع مورد نظر برای جلسه")]
        public string? NeedArea { get; set; } = string.Empty;

        [Display(Name = "زمان پیشنهادی جلسه")]
        public string? PreferredTime { get; set; } = string.Empty;

        [Display(Name = "توضیح کوتاه")]
        public string? Notes { get; set; } = string.Empty;
    }
}
