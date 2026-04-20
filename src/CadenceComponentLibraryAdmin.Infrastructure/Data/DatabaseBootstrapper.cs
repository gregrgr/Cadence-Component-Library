using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data;

public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
        if (migrationsAssembly.Migrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync(cancellationToken))
            {
                await databaseCreator.CreateAsync(cancellationToken);
            }

            if (!await TableExistsAsync(dbContext, "AspNetRoles", cancellationToken))
            {
                await databaseCreator.CreateTablesAsync(cancellationToken);
            }
        }

        await InstallViewsAsync(dbContext, cancellationToken);
    }

    public static async Task VerifyDatabaseStateAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "The application database is not reachable. Apply EF Core migrations before starting in non-development environments.");
        }

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pendingMigrations.Count > 0)
        {
            throw new InvalidOperationException(
                $"Pending EF Core migrations were found: {string.Join(", ", pendingMigrations)}. Apply migrations explicitly before starting the application.");
        }

        var installedViews = await GetInstalledViewsAsync(dbContext, cancellationToken);
        var missingViews = RequiredViews.Except(installedViews, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingViews.Count > 0)
        {
            throw new InvalidOperationException(
                $"Required CIS SQL views are missing: {string.Join(", ", missingViews)}. Reapply the database baseline before starting the application.");
        }
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext dbContext, string tableName, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @tableName
            ) THEN 1 ELSE 0 END;
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    private static async Task InstallViewsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        foreach (var sql in GetViewStatements())
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }

    internal static IReadOnlyList<string> GetViewStatements()
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Views", "CisViews.sql");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Unable to locate CIS view script at '{scriptPath}'.", scriptPath);
        }

        var statements = new List<string>();
        var current = new StringBuilder();

        foreach (var line in File.ReadAllLines(scriptPath))
        {
            if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
            {
                AddStatement(statements, current);
                continue;
            }

            current.AppendLine(line);
        }

        AddStatement(statements, current);
        return statements;
    }

    internal static async Task<IReadOnlyList<string>> GetInstalledViewsAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Database
            .SqlQueryRaw<string>("SELECT TABLE_NAME AS [Value] FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = 'dbo'")
            .ToListAsync(cancellationToken);
    }

    internal static readonly string[] RequiredViews =
    [
        "vw_CIS_Release_Parts",
        "vw_CIS_Alternates"
    ];

    private static void AddStatement(List<string> statements, StringBuilder current)
    {
        var sql = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(sql))
        {
            statements.Add(sql);
        }

        current.Clear();
    }
}
