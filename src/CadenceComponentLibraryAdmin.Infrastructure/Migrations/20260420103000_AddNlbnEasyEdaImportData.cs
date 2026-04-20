using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    public partial class AddNlbnEasyEdaImportData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EasyEdaCParaJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EasyEdaDataStrRawJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EasyEdaLcscRawJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EasyEdaPackageDetailRawJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EasyEdaRawJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FootprintBBoxX",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FootprintBBoxY",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FootprintShapeJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JlcPartClass",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SymbolBBoxX",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SymbolBBoxY",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SymbolShapeJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "FootprintPreviewAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "ObjAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "FootprintPreviewAssetId",
                principalSchema: "dbo",
                principalTable: "ExternalComponentAssets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "ObjAssetId",
                principalSchema: "dbo",
                principalTable: "ExternalComponentAssets",
                principalColumn: "Id");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "ExternalImportSources",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Notes", "SourceName", "SourceType" },
                values: new object[] { "Seeded import source for the nlbn-style EasyEDA/LCSC staging connector.", "EasyEDA/LCSC", 2 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalComponentImports_ExternalComponentAssets_FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalComponentImports_ExternalComponentAssets_ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropIndex(
                name: "IX_ExternalComponentImports_FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropIndex(
                name: "IX_ExternalComponentImports_ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "EasyEdaCParaJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "EasyEdaDataStrRawJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "EasyEdaLcscRawJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "EasyEdaPackageDetailRawJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "EasyEdaRawJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "FootprintBBoxX",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "FootprintBBoxY",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "FootprintPreviewAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "FootprintShapeJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "JlcPartClass",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "ObjAssetId",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "PackageName",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "SymbolBBoxX",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "SymbolBBoxY",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "SymbolShapeJson",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "ExternalImportSources",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Notes", "SourceName", "SourceType" },
                values: new object[] { "Seeded import source for the EasyEDA Pro extension staging connector.", "EasyEDA Pro", 1 });
        }
    }
}
