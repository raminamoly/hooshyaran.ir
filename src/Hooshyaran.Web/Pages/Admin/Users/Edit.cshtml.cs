using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Users;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class EditModel(HooshyaranDbContext dbContext, IPasswordHasher passwordHasher) : PageModel
{
    [BindProperty]
    public UserInput Input { get; set; } = new();

    public List<SelectListItem> RoleOptions { get; } =
    [
        new("SuperAdmin", AdminUserRoles.SuperAdmin),
        new("نویسنده", AdminUserRoles.Author)
    ];

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            Input.IsActive = true;
            Input.Role = AdminUserRoles.Author;
            return Page();
        }

        var user = await dbContext.AdminUsers.FindAsync(id.Value);
        if (user is null)
        {
            return NotFound();
        }

        Input = new UserInput
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Id == 0 && string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.AddModelError("Input.Password", "رمز عبور برای کاربر جدید الزامی است.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Role is not (AdminUserRoles.SuperAdmin or AdminUserRoles.Author))
        {
            ModelState.AddModelError("Input.Role", "نقش انتخاب‌شده معتبر نیست.");
            return Page();
        }

        if (await dbContext.AdminUsers.AnyAsync(user => user.UserName == Input.UserName && user.Id != Input.Id))
        {
            ModelState.AddModelError("Input.UserName", "این نام کاربری قبلا استفاده شده است.");
            return Page();
        }

        var userEntity = Input.Id == 0
            ? new AdminUser { CreatedAt = DateTimeOffset.UtcNow }
            : await dbContext.AdminUsers.FindAsync(Input.Id);

        if (userEntity is null)
        {
            return NotFound();
        }

        userEntity.UserName = Input.UserName.Trim();
        userEntity.DisplayName = Input.DisplayName.Trim();
        userEntity.Email = Input.Email.Trim();
        userEntity.Role = Input.Role;
        userEntity.IsActive = Input.IsActive;
        userEntity.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(Input.Password))
        {
            userEntity.PasswordHash = passwordHasher.HashPassword(Input.Password);
        }

        if (Input.Id == 0)
        {
            dbContext.AdminUsers.Add(userEntity);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Users/Index");
    }

    public class UserInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام نمایشی الزامی است.")]
        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام کاربری الزامی است.")]
        [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "نام کاربری فقط شامل حروف انگلیسی، عدد، نقطه، خط تیره و زیرخط باشد.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "نقش الزامی است.")]
        [Display(Name = "نقش")]
        public string Role { get; set; } = AdminUserRoles.Author;

        public bool IsActive { get; set; } = true;
    }
}
