using CadenceComponentLibraryAdmin.Infrastructure.Data;
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
}
