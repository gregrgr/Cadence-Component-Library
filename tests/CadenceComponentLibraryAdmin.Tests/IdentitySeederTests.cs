using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class IdentitySeederTests
{
    [Fact]
    public async Task SeedAsync_SkipsDefaultAdminOutsideDevelopment()
    {
        await using var dbContext = CreateDbContext();
        using var roleManager = CreateRoleManager(dbContext);
        using var userManager = CreateUserManager(dbContext);

        await IdentitySeeder.SeedAsync(roleManager, userManager, seedDefaultAdmin: false);

        foreach (var role in IdentitySeedData.Roles)
        {
            Assert.True(await roleManager.RoleExistsAsync(role));
        }

        Assert.Null(await userManager.FindByEmailAsync(IdentitySeedData.AdminEmail));
    }

    [Fact]
    public async Task SeedAsync_CreatesDefaultAdminInDevelopment()
    {
        await using var dbContext = CreateDbContext();
        using var roleManager = CreateRoleManager(dbContext);
        using var userManager = CreateUserManager(dbContext);

        await IdentitySeeder.SeedAsync(roleManager, userManager, seedDefaultAdmin: true);
        var adminUser = await userManager.FindByEmailAsync(IdentitySeedData.AdminEmail);

        Assert.NotNull(adminUser);
        Assert.True(await userManager.IsInRoleAsync(adminUser!, "Admin"));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RoleManager<IdentityRole> CreateRoleManager(ApplicationDbContext dbContext)
    {
        var roleStore = new RoleStore<IdentityRole, ApplicationDbContext, string>(dbContext);
        return new RoleManager<IdentityRole>(
            roleStore,
            [new RoleValidator<IdentityRole>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole>>.Instance);
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext dbContext)
    {
        var userStore = new UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string>(dbContext);
        var identityOptions = new IdentityOptions
        {
            Password =
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredLength = 8
            }
        };

        return new UserManager<ApplicationUser>(
            userStore,
            Options.Create(identityOptions),
            new PasswordHasher<ApplicationUser>(),
            [new UserValidator<ApplicationUser>()],
            [new PasswordValidator<ApplicationUser>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }
}
