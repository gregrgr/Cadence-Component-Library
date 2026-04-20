using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class DatabaseBootstrapperTests
{
    [Fact]
    public void GetViewStatements_ContainsExpectedCadenceViews()
    {
        var statements = DatabaseBootstrapper.GetViewStatements();

        Assert.Equal(2, statements.Count);
        Assert.Contains(statements, sql => sql.Contains("CREATE OR ALTER VIEW dbo.vw_CIS_Release_Parts", StringComparison.Ordinal));
        Assert.Contains(statements, sql => sql.Contains("CREATE OR ALTER VIEW dbo.vw_CIS_Alternates", StringComparison.Ordinal));
        Assert.All(statements, sql => Assert.DoesNotContain("\nGO", sql, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InitializeAsync_CreatesViewsInSqlServer_WhenConnectionStringIsAvailable()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            return;
        }

        var databaseName = $"CadenceBootstrapperTests_{Guid.NewGuid():N}";
        var connectionString = $"{baseConnectionString};Initial Catalog={databaseName}";

        await using var dbContext = CreateSqlServerDbContext(connectionString);

        try
        {
            await DatabaseBootstrapper.InitializeAsync(dbContext);

            var viewNames = await dbContext.Database
                .SqlQueryRaw<string>("SELECT TABLE_NAME AS [Value] FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = 'dbo'")
                .ToListAsync();

            Assert.Contains("vw_CIS_Release_Parts", viewNames);
            Assert.Contains("vw_CIS_Alternates", viewNames);
        }
        finally
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    private static ApplicationDbContext CreateSqlServerDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
