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
