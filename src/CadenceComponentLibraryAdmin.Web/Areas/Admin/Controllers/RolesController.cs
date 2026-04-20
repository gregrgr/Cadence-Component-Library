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
public sealed class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminAuditService _adminAuditService;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IAdminAuditService adminAuditService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _adminAuditService = adminAuditService;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
        var items = new List<AdminRoleListItemViewModel>(roles.Count);
        foreach (var role in roles)
        {
            var userNames = role.Name is null
                ? []
                : (await _userManager.GetUsersInRoleAsync(role.Name))
                    .Select(x => x.Email ?? x.UserName ?? x.Id)
                    .OrderBy(x => x)
                    .ToList();

            items.Add(new AdminRoleListItemViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                IsSystemRole = IsSystemRole(role.Name),
                Users = userNames
            });
        }

        return View(new AdminRolesIndexViewModel
        {
            Roles = items
        });
    }

    public IActionResult Create()
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(new AdminRoleCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminRoleCreateViewModel model)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var roleName = model.Name.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(nameof(model.Name), "Role name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
            return View(model);
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var role = await _roleManager.FindByNameAsync(roleName);
        await _adminAuditService.WriteAsync(
            "RoleCreated",
            "Role",
            role?.Id ?? roleName,
            roleName,
            null,
            roleName,
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var assignedUsers = role.Name is null
            ? []
            : (await _userManager.GetUsersInRoleAsync(role.Name))
                .Select(x => x.Email ?? x.UserName ?? x.Id)
                .OrderBy(x => x)
                .ToList();

        return View(new AdminRoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            IsSystemRole = IsSystemRole(role.Name),
            AssignedUsers = assignedUsers
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminRoleEditViewModel model)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (id != model.Id)
        {
            return NotFound();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var currentName = role.Name ?? string.Empty;
        var newName = model.Name.Trim();
        var assignedUsers = currentName.Length == 0
            ? []
            : await _userManager.GetUsersInRoleAsync(currentName);

        if (IsSystemRole(currentName))
        {
            TempData["ErrorMessage"] = "Seeded system roles cannot be renamed.";
            return RedirectToAction(nameof(Index));
        }

        if (assignedUsers.Count > 0)
        {
            TempData["ErrorMessage"] = "Only unassigned custom roles can be renamed safely.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            ModelState.AddModelError(nameof(model.Name), "Role name is required.");
        }

        if (!string.Equals(currentName, newName, StringComparison.OrdinalIgnoreCase) &&
            await _roleManager.RoleExistsAsync(newName))
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            model = model.WithAssignedUsers(assignedUsers.Select(x => x.Email ?? x.UserName ?? x.Id).ToList(), IsSystemRole(currentName));
            return View(model);
        }

        role.Name = newName;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            model = model.WithAssignedUsers(assignedUsers.Select(x => x.Email ?? x.UserName ?? x.Id).ToList(), false);
            return View(model);
        }

        await _adminAuditService.WriteAsync(
            "RoleRenamed",
            "Role",
            role.Id,
            newName,
            currentName,
            newName,
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "Role renamed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var roleName = role.Name ?? string.Empty;
        if (IsSystemRole(roleName))
        {
            TempData["ErrorMessage"] = "Seeded system roles cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        if (roleName.Length > 0 && (await _userManager.GetUsersInRoleAsync(roleName)).Count > 0)
        {
            TempData["ErrorMessage"] = "Roles with assigned users cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(x => x.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminAuditService.WriteAsync(
            "RoleDeleted",
            "Role",
            role.Id,
            roleName,
            roleName,
            null,
            GetActor(),
            GetIpAddress(),
            GetUserAgent());

        TempData["SuccessMessage"] = "Role deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static bool IsSystemRole(string? roleName)
        => !string.IsNullOrWhiteSpace(roleName)
           && IdentitySeedData.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    private string GetActor() => User.Identity?.Name ?? "system";

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent() => Request.Headers.UserAgent.ToString();
}

file static class AdminRoleEditViewModelExtensions
{
    public static AdminRoleEditViewModel WithAssignedUsers(
        this AdminRoleEditViewModel model,
        IReadOnlyList<string> assignedUsers,
        bool isSystemRole)
    {
        return new AdminRoleEditViewModel
        {
            Id = model.Id,
            Name = model.Name,
            IsSystemRole = isSystemRole,
            AssignedUsers = assignedUsers
        };
    }
}
