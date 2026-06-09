using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages;

public class RequestDemoModel(
    HooshyaranDbContext dbContext,
    IDemoRequestEmailService demoRequestEmailService,
    ISiteVisitLogger visitLogger) : PageModel
{
    [BindProperty]
    public DemoInput Input { get; set; } = new();

    public bool Submitted { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var demoRequest = new DemoRequest
        {
            FullName = Input.FullName.Trim(),
            OrganizationName = Input.OrganizationName.Trim(),
            JobTitle = Input.JobTitle.Trim(),
            PhoneNumber = Input.PhoneNumber.Trim(),
            Email = Input.Email.Trim(),
            NeedArea = Input.NeedArea.Trim(),
            PreferredTime = Input.PreferredTime.Trim(),
            Notes = Input.Notes.Trim(),
            Status = DemoRequestStatuses.New,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DemoRequests.Add(demoRequest);
        await dbContext.SaveChangesAsync();
        await visitLogger.LogAsync(HttpContext, SiteVisitEventTypes.DemoSubmit, "ثبت فرم درخواست دمو");
        await demoRequestEmailService.NotifyAsync(demoRequest);

        ModelState.Clear();
        Input = new DemoInput();
        Submitted = true;
        return Page();
    }

    public class DemoInput
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام سازمان یا شرکت الزامی است.")]
        [Display(Name = "نام سازمان/شرکت")]
        public string OrganizationName { get; set; } = string.Empty;

        [Display(Name = "سمت")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل کاری")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "حوزه نیاز")]
        public string NeedArea { get; set; } = string.Empty;

        [Display(Name = "زمان پیشنهادی جلسه")]
        public string PreferredTime { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        public string Notes { get; set; } = string.Empty;
    }
}
