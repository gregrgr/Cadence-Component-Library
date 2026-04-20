using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalImportTokensAndLcscEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LcscEnrichedAt",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LcscEnrichmentMessage",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LcscEnrichmentStatus",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LcscRawJson",
                schema: "dbo",
                table: "ExternalComponentImports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalImportTokens",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedByUserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AllowedOrigins = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalImportTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalImportTokens_SourceName_ExpiresAt",
                schema: "dbo",
                table: "ExternalImportTokens",
                columns: new[] { "SourceName", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalImportTokens_TokenHash",
                schema: "dbo",
                table: "ExternalImportTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalImportTokens",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "LcscEnrichedAt",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "LcscEnrichmentMessage",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "LcscEnrichmentStatus",
                schema: "dbo",
                table: "ExternalComponentImports");

            migrationBuilder.DropColumn(
                name: "LcscRawJson",
                schema: "dbo",
                table: "ExternalComponentImports");
        }
    }
}
