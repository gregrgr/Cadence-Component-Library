using CadenceComponentLibraryAdmin.Infrastructure.Services;
using CadenceComponentLibraryAdmin.Web.Areas.Admin.Controllers;
using CadenceComponentLibraryAdmin.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class AdminUsersAndRolesControllerTests
{
    [Fact]
    public async Task Admin_CanAccessUsersIndex()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);

        var admin = await IdentityManagementTestHelper.CreateUserAsync(userManager, "admin@test.local", "Admin");
        var controller = new UsersController(userManager, roleManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, admin.Id, admin.Email!, "Admin");

        var result = await controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<AdminUsersIndexViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task NonAdmin_CannotAccessUsersIndex()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);

        var viewer = await IdentityManagementTestHelper.CreateUserAsync(userManager, "viewer@test.local", "Viewer");
        var controller = new UsersController(userManager, roleManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, viewer.Id, viewer.Email!, "Viewer");

        var result = await controller.Index(null);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CurrentAdmin_CannotRemoveOwnAdminRole()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);

        var admin = await IdentityManagementTestHelper.CreateUserAsync(userManager, "admin@test.local", "Admin", "Viewer");
        var controller = new UsersController(userManager, roleManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, admin.Id, admin.Email!, "Admin");

        var result = await controller.Edit(admin.Id, new AdminUserEditViewModel
        {
            Id = admin.Id,
            Email = admin.Email!,
            UserName = admin.UserName!,
            EmailConfirmed = true,
            LockoutEnabled = true,
            SelectedRoles = ["Viewer"]
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors, error => error.ErrorMessage.Contains("own Admin role", StringComparison.Ordinal));
        Assert.IsType<AdminUserEditViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task CannotRemoveLastActiveAdmin()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);

        var soleAdmin = await IdentityManagementTestHelper.CreateUserAsync(userManager, "sole-admin@test.local", "Admin");
        var controller = new UsersController(userManager, roleManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, "actor-not-in-store", "external-admin@test.local", "Admin");

        var result = await controller.Edit(soleAdmin.Id, new AdminUserEditViewModel
        {
            Id = soleAdmin.Id,
            Email = soleAdmin.Email!,
            UserName = soleAdmin.UserName!,
            EmailConfirmed = true,
            LockoutEnabled = true,
            SelectedRoles = []
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors, error => error.ErrorMessage.Contains("active Admin user must remain", StringComparison.Ordinal));
        Assert.IsType<AdminUserEditViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task SeededRoles_CannotBeDeleted()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);
        var admin = await IdentityManagementTestHelper.CreateUserAsync(userManager, "admin@test.local", "Admin");
        var adminRole = await roleManager.FindByNameAsync("Admin");
        Assert.NotNull(adminRole);

        var controller = new RolesController(roleManager, userManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, admin.Id, admin.Email!, "Admin");

        var result = await controller.Delete(adminRole!.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(await roleManager.RoleExistsAsync("Admin"));
        Assert.Equal("Seeded system roles cannot be deleted.", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task UserRoleChange_WritesAdminAuditLog()
    {
        await using var dbContext = IdentityManagementTestHelper.CreateDbContext();
        using var roleManager = IdentityManagementTestHelper.CreateRoleManager(dbContext);
        using var userManager = IdentityManagementTestHelper.CreateUserManager(dbContext);
        await IdentityManagementTestHelper.SeedRolesAsync(roleManager);

        var admin = await IdentityManagementTestHelper.CreateUserAsync(userManager, "admin@test.local", "Admin");
        var user = await IdentityManagementTestHelper.CreateUserAsync(userManager, "user@test.local", "Viewer");
        var controller = new UsersController(userManager, roleManager, new AdminAuditService(dbContext));
        IdentityManagementTestHelper.AttachControllerContext(controller, admin.Id, admin.Email!, "Admin");

        var result = await controller.Edit(user.Id, new AdminUserEditViewModel
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            EmailConfirmed = true,
            LockoutEnabled = true,
            SelectedRoles = ["Librarian"]
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains(dbContext.AdminAuditLogs, log => log.Action == "RolesChanged" && log.TargetId == user.Id);
    }
}
