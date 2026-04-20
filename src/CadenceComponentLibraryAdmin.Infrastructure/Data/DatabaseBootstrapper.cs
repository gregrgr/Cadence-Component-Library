using System.Data;
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

    internal static IReadOnlyList<string> GetViewStatements() =>
    [
        """
        CREATE OR ALTER VIEW dbo.vw_CIS_Release_Parts AS
        SELECT
            cp.CompanyPN              AS COMPANY_PN,
            cp.PartClass              AS PART_CLASS,
            cp.Description            AS DESCRIPTION,
            cp.ValueNorm              AS VALUE,
            mp.Manufacturer           AS MANUFACTURER_NAME,
            mp.ManufacturerPN         AS MANUFACTURER_PART_NUMBER,
            sf.SymbolName             AS SCHEMATIC_PART,
            sf.OlbPath                AS SCHEMATIC_LIBRARY,
            cp.DefaultFootprintName   AS PCB_FOOTPRINT,
            cp.PackageFamilyCode      AS PACKAGE_FAMILY,
            cp.AltGroup               AS ALT_GROUP,
            cp.ApprovalStatus         AS APPROVAL_STATUS,
            cp.LifecycleStatus        AS LIFECYCLE_STATUS,
            cp.RoHS                   AS ROHS,
            cp.REACHStatus            AS REACH,
            cp.HeightMaxMm            AS HEIGHT_MAX_MM,
            cp.TempRange              AS TEMP_RANGE,
            cp.DatasheetUrl           AS DATASHEET_URL,
            fv.StepPath               AS STEP_MODEL,
            fv.Status                 AS FOOTPRINT_STATUS
        FROM dbo.CompanyParts cp
        JOIN dbo.ManufacturerParts mp
          ON mp.CompanyPN = cp.CompanyPN
         AND mp.IsApproved = 1
        JOIN dbo.SymbolFamilies sf
          ON sf.SymbolFamilyCode = cp.SymbolFamilyCode
         AND sf.IsActive = 1
        JOIN dbo.FootprintVariants fv
          ON fv.FootprintName = cp.DefaultFootprintName
         AND fv.Status = 2
        WHERE cp.ApprovalStatus = 2
          AND cp.LifecycleStatus NOT IN (3, 4);
        """,
        """
        CREATE OR ALTER VIEW dbo.vw_CIS_Alternates AS
        SELECT
            a.SourceCompanyPN,
            src.Description AS SourceDescription,
            a.TargetCompanyPN,
            tgt.Description AS TargetDescription,
            a.AltLevel,
            a.SameFootprintYN,
            src.DefaultFootprintName AS SourceFootprint,
            tgt.DefaultFootprintName AS TargetFootprint,
            a.NeedEEReviewYN,
            a.NeedLayoutReviewYN
        FROM dbo.PartAlternates a
        JOIN dbo.CompanyParts src
          ON src.CompanyPN = a.SourceCompanyPN
        JOIN dbo.CompanyParts tgt
          ON tgt.CompanyPN = a.TargetCompanyPN
        WHERE src.ApprovalStatus = 2
          AND tgt.ApprovalStatus = 2;
        """
    ];
}
