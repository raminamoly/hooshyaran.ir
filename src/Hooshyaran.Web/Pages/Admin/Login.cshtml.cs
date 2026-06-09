using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin;

public class LoginModel(HooshyaranDbContext dbContext, IPasswordHasher passwordHasher) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Admin/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var admin = await dbContext.AdminUsers
            .SingleOrDefaultAsync(user => user.UserName == Input.UserName && user.IsActive);

        if (admin is null || !passwordHasher.VerifyPassword(Input.Password, admin.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور معتبر نیست.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Name, admin.UserName),
            new(ClaimTypes.Role, admin.Role),
            new(ClaimTypes.Email, admin.Email),
            new("DisplayName", admin.DisplayName)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToPage("/Admin/Index");
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "نام کاربری الزامی است.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = string.Empty;
    }
}
