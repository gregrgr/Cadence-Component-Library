using CadenceComponentLibraryAdmin.Application.Interfaces;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class ExternalImportTokenServiceTests
{
    [Fact]
    public async Task RawToken_IsNotStoredAtRest()
    {
        await using var dbContext = CreateDbContext();
        var service = new ExternalImportTokenService(dbContext);

        var result = await service.CreateTokenAsync(
            new ExternalImportTokenCreateRequest("token-1", "EasyEDA Pro", DateTime.UtcNow.AddDays(30), null, null),
            "user-1",
            "admin@local.test");

        var stored = await dbContext.ExternalImportTokens.SingleAsync();
        Assert.NotEqual(result.RawToken, stored.TokenHash);
        Assert.Equal(ExternalImportTokenService.HashToken(result.RawToken), stored.TokenHash);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
