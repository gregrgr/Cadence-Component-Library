using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CadenceComponentLibraryAdmin.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        bool seedDefaultAdmin,
        BootstrapAdminOptions? bootstrapAdminOptions = null)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedAsync(roleManager, userManager, seedDefaultAdmin, bootstrapAdminOptions);
    }

    internal static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        bool seedDefaultAdmin,
        BootstrapAdminOptions? bootstrapAdminOptions = null)
    {

        foreach (var role in IdentitySeedData.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }
            }
        }

        if (seedDefaultAdmin)
        {
            await SeedAdminAsync(userManager, IdentitySeedData.AdminEmail, IdentitySeedData.AdminPassword);
            return;
        }

        if (bootstrapAdminOptions?.Enabled != true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(bootstrapAdminOptions.Email) ||
            string.IsNullOrWhiteSpace(bootstrapAdminOptions.Password))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin is enabled, but BootstrapAdmin:Email or BootstrapAdmin:Password is missing.");
        }

        await SeedAdminAsync(userManager, bootstrapAdminOptions.Email, bootstrapAdminOptions.Password);
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password)
    {
        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var result = await userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to create initial admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to '{email}': {errors}");
            }
        }
    }
}
