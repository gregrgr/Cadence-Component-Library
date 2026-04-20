using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260420124000_RepairNlbnEasyEdaImportData")]
    public partial class RepairNlbnEasyEdaImportData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaCParaJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD EasyEdaCParaJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaDataStrRawJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD EasyEdaDataStrRawJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaLcscRawJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD EasyEdaLcscRawJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaPackageDetailRawJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD EasyEdaPackageDetailRawJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaRawJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD EasyEdaRawJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintBBoxX') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD FootprintBBoxX decimal(18,6) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintBBoxY') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD FootprintBBoxY decimal(18,6) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintPreviewAssetId') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD FootprintPreviewAssetId bigint NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintShapeJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD FootprintShapeJson nvarchar(max) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'JlcPartClass') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD JlcPartClass nvarchar(200) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'ObjAssetId') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD ObjAssetId bigint NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'PackageName') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD PackageName nvarchar(255) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolBBoxX') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD SymbolBBoxX decimal(18,6) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolBBoxY') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD SymbolBBoxY decimal(18,6) NULL;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolShapeJson') IS NULL
                    ALTER TABLE dbo.ExternalComponentImports ADD SymbolShapeJson nvarchar(max) NULL;
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_ExternalComponentImports_FootprintPreviewAssetId'
                      AND object_id = OBJECT_ID('dbo.ExternalComponentImports')
                )
                CREATE INDEX IX_ExternalComponentImports_FootprintPreviewAssetId
                    ON dbo.ExternalComponentImports (FootprintPreviewAssetId);

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_ExternalComponentImports_ObjAssetId'
                      AND object_id = OBJECT_ID('dbo.ExternalComponentImports')
                )
                CREATE INDEX IX_ExternalComponentImports_ObjAssetId
                    ON dbo.ExternalComponentImports (ObjAssetId);
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId'
                )
                ALTER TABLE dbo.ExternalComponentImports
                    ADD CONSTRAINT FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId
                    FOREIGN KEY (FootprintPreviewAssetId)
                    REFERENCES dbo.ExternalComponentAssets (Id);

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId'
                )
                ALTER TABLE dbo.ExternalComponentImports
                    ADD CONSTRAINT FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId
                    FOREIGN KEY (ObjAssetId)
                    REFERENCES dbo.ExternalComponentAssets (Id);
                """);

            migrationBuilder.Sql(
                """
                UPDATE dbo.ExternalImportSources
                SET SourceName = N'EasyEDA/LCSC',
                    SourceType = 2,
                    Notes = N'Seeded import source for the nlbn-style EasyEDA/LCSC staging connector.'
                WHERE Id = 1
                  AND (
                      SourceName <> N'EasyEDA/LCSC'
                      OR SourceType <> 2
                      OR Notes <> N'Seeded import source for the nlbn-style EasyEDA/LCSC staging connector.'
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId'
                )
                ALTER TABLE dbo.ExternalComponentImports
                    DROP CONSTRAINT FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId;

                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId'
                )
                ALTER TABLE dbo.ExternalComponentImports
                    DROP CONSTRAINT FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId;

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_ExternalComponentImports_FootprintPreviewAssetId'
                      AND object_id = OBJECT_ID('dbo.ExternalComponentImports')
                )
                DROP INDEX IX_ExternalComponentImports_FootprintPreviewAssetId ON dbo.ExternalComponentImports;

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_ExternalComponentImports_ObjAssetId'
                      AND object_id = OBJECT_ID('dbo.ExternalComponentImports')
                )
                DROP INDEX IX_ExternalComponentImports_ObjAssetId ON dbo.ExternalComponentImports;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaCParaJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN EasyEdaCParaJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaDataStrRawJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN EasyEdaDataStrRawJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaLcscRawJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN EasyEdaLcscRawJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaPackageDetailRawJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN EasyEdaPackageDetailRawJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'EasyEdaRawJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN EasyEdaRawJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintBBoxX') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN FootprintBBoxX;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintBBoxY') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN FootprintBBoxY;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintPreviewAssetId') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN FootprintPreviewAssetId;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'FootprintShapeJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN FootprintShapeJson;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'JlcPartClass') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN JlcPartClass;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'ObjAssetId') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN ObjAssetId;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'PackageName') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN PackageName;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolBBoxX') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN SymbolBBoxX;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolBBoxY') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN SymbolBBoxY;

                IF COL_LENGTH('dbo.ExternalComponentImports', 'SymbolShapeJson') IS NOT NULL
                    ALTER TABLE dbo.ExternalComponentImports DROP COLUMN SymbolShapeJson;
                """);
        }
    }
}
