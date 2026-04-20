using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Seed;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAdminAuditService _adminAuditService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAdminAuditService adminAuditService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminAuditService = adminAuditService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var query = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.Email != null && x.Email.Contains(search)) ||
                (x.UserName != null && x.UserName.Contains(search)));
        }

        var users = await query.OrderBy(x => x.Email).ToListAsync();
        var items = new List<AdminUserListItemViewModel>(users.Count);
        foreach (var user in users)
        {
            items.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Roles = (await _userManager.GetRolesAsync(user)).OrderBy(x => x).ToList(),
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount
            });
        }

        return View(new AdminUsersIndexViewModel
        {
            Search = search,
            Users = items
        });
    }

    public async Task<IActionResult> Create()
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(await BuildCreateViewModelAsync(new AdminUserCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateViewModel model)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildCreateViewModelAsync(model));
        }

        var validRoles = await NormalizeSelectedRolesAsync(model.SelectedRoles);
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(await BuildCreateViewModelAsync(model));
        }

        if (validRoles.Count > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(user, validRoles);
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return View(await BuildCreateViewModelAsync(model));
            }
        }

        await _adminAuditService.WriteAsync(
            "UserCreated",
            "User",
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            null,
            $"Roles: {FormatRoles(validRoles)}",
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(await BuildEditViewModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminUserEditViewModel model)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (id != model.Id)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildEditViewModelAsync(user, model));
        }

        var selectedRoles = await NormalizeSelectedRolesAsync(model.SelectedRoles);
        var existingRoles = (await _userManager.GetRolesAsync(user)).ToList();
        var roleValidationError = await ValidateRoleChangeAsync(user, existingRoles, selectedRoles);
        if (roleValidationError is not null)
        {
            ModelState.AddModelError(string.Empty, roleValidationError);
            return View(await BuildEditViewModelAsync(user, model));
        }

        var oldState = $"Email={user.Email}; UserName={user.UserName}; EmailConfirmed={user.EmailConfirmed}; LockoutEnabled={user.LockoutEnabled}; Roles={FormatRoles(existingRoles)}";

        user.Email = model.Email;
        user.UserName = model.UserName;
        user.EmailConfirmed = model.EmailConfirmed;
        user.LockoutEnabled = model.LockoutEnabled;
        if (!model.LockoutEnabled)
        {
            user.LockoutEnd = null;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(await BuildEditViewModelAsync(user, model));
        }

        var rolesToAdd = selectedRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        var rolesToRemove = existingRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                AddIdentityErrors(removeResult);
                return View(await BuildEditViewModelAsync(user, model));
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                AddIdentityErrors(addResult);
                return View(await BuildEditViewModelAsync(user, model));
            }
        }

        var newState = $"Email={user.Email}; UserName={user.UserName}; EmailConfirmed={user.EmailConfirmed}; LockoutEnabled={user.LockoutEnabled}; Roles={FormatRoles(selectedRoles)}";
        await _adminAuditService.WriteAsync(
            "UserUpdated",
            "User",
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            oldState,
            newState,
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        if (rolesToAdd.Length > 0 || rolesToRemove.Length > 0)
        {
            await _adminAuditService.WriteAsync(
                "RolesChanged",
                "User",
                user.Id,
                user.Email ?? user.UserName ?? user.Id,
                FormatRoles(existingRoles),
                FormatRoles(selectedRoles),
                GetActor(),
                GetIpAddress(),
                GetUserAgent());
        }

        TempData["SuccessMessage"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new AdminResetPasswordViewModel
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? user.Id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, AdminResetPasswordViewModel model)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (id != model.Id)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        await _adminAuditService.WriteAsync(
            "PasswordReset",
            "User",
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            null,
            "Password reset by administrator",
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "Password reset.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot lock the currently signed-in administrator.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin") && await GetActiveAdminCountAsync(excludingUserId: user.Id) == 0)
        {
            TempData["ErrorMessage"] = "At least one active Admin user must remain.";
            return RedirectToAction(nameof(Index));
        }

        user.LockoutEnabled = true;
        await _userManager.UpdateAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

        await _adminAuditService.WriteAsync(
            "UserLocked",
            "User",
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            "Unlocked",
            "Locked",
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "User locked.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);

        await _adminAuditService.WriteAsync(
            "UserUnlocked",
            "User",
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            "Locked",
            "Unlocked",
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "User unlocked.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminUserCreateViewModel> BuildCreateViewModelAsync(AdminUserCreateViewModel model)
    {
        model.AvailableRoles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToListAsync();
        model.SelectedRoles = model.SelectedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return model;
    }

    private async Task<AdminUserEditViewModel> BuildEditViewModelAsync(ApplicationUser user, AdminUserEditViewModel? source = null)
    {
        var selectedRoles = source?.SelectedRoles ?? (await _userManager.GetRolesAsync(user)).ToList();
        return new AdminUserEditViewModel
        {
            Id = user.Id,
            Email = source?.Email ?? user.Email ?? string.Empty,
            UserName = source?.UserName ?? user.UserName ?? string.Empty,
            EmailConfirmed = source?.EmailConfirmed ?? user.EmailConfirmed,
            LockoutEnabled = source?.LockoutEnabled ?? user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            AvailableRoles = await _roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name!).ToListAsync(),
            SelectedRoles = selectedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    private async Task<List<string>> NormalizeSelectedRolesAsync(IEnumerable<string>? selectedRoles)
    {
        var allRoles = await _roleManager.Roles.Select(x => x.Name!).ToListAsync();
        return (selectedRoles ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(role => allRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x)
            .ToList();
    }

    private async Task<string?> ValidateRoleChangeAsync(ApplicationUser user, IReadOnlyCollection<string> currentRoles, IReadOnlyCollection<string> selectedRoles)
    {
        var currentUserId = _userManager.GetUserId(User);
        var removingAdmin = currentRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase)
            && !selectedRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

        if (user.Id == currentUserId && removingAdmin)
        {
            return "You cannot remove your own Admin role.";
        }

        if (removingAdmin && await GetActiveAdminCountAsync(excludingUserId: user.Id) == 0)
        {
            return "At least one active Admin user must remain.";
        }

        return null;
    }

    private async Task<int> GetActiveAdminCountAsync(string? excludingUserId = null)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.Count(user =>
            !string.Equals(user.Id, excludingUserId, StringComparison.Ordinal) &&
            (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private string GetActor() => User.Identity?.Name ?? "system";

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent() => Request.Headers.UserAgent.ToString();

    private static string FormatRoles(IEnumerable<string> roles)
        => string.Join(", ", roles.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
}
