using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_SetsCreatedAndUpdatedAuditFields()
    {
        await using var dbContext = CreateDbContext();

        var symbolFamily = new SymbolFamily
        {
            SymbolFamilyCode = "RES",
            SymbolName = "RES_2PIN",
            OlbPath = "Symbols_OLB/Passive/passive.olb",
            PartClass = "Passive",
            IsActive = true
        };

        dbContext.SymbolFamilies.Add(symbolFamily);
        await dbContext.SaveChangesAsync();

        Assert.NotEqual(default, symbolFamily.CreatedAt);
        Assert.Null(symbolFamily.UpdatedAt);

        symbolFamily.SymbolName = "RES_2PIN_UPDATED";
        await dbContext.SaveChangesAsync();

        Assert.NotNull(symbolFamily.UpdatedAt);
        Assert.True(symbolFamily.UpdatedAt >= symbolFamily.CreatedAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
