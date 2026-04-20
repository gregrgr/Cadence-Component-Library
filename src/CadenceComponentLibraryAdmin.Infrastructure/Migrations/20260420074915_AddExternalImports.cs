using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalImportSources",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalImportSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalComponentAssets",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ExternalComponentImportId = table.Column<long>(type: "bigint", nullable: true),
                    AssetType = table.Column<int>(type: "int", nullable: false),
                    ExternalUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    RawMetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalComponentAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalComponentImports",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ExternalDeviceUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExternalLibraryUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SearchKeyword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LcscId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImportKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ClassificationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ManufacturerPN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    SupplierId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SymbolName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SymbolUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SymbolLibraryUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SymbolType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SymbolRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FootprintName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FootprintUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FootprintLibraryUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FootprintRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FootprintRenderAssetId = table.Column<long>(type: "bigint", nullable: true),
                    ImageUuidsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model3DName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Model3DUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Model3DLibraryUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Model3DRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatasheetUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManualUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StepUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StepAssetId = table.Column<long>(type: "bigint", nullable: true),
                    DatasheetAssetId = table.Column<long>(type: "bigint", nullable: true),
                    ManualAssetId = table.Column<long>(type: "bigint", nullable: true),
                    JlcInventory = table.Column<long>(type: "bigint", nullable: true),
                    JlcPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    LcscInventory = table.Column<long>(type: "bigint", nullable: true),
                    LcscPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    SearchItemRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceItemRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceAssociationRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DevicePropertyRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherPropertyRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullRawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportStatus = table.Column<int>(type: "int", nullable: false),
                    DuplicateWarning = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CandidateId = table.Column<long>(type: "bigint", nullable: true),
                    LastImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalComponentImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalComponentImports_ExternalComponentAssets_DatasheetAssetId",
                        column: x => x.DatasheetAssetId,
                        principalSchema: "dbo",
                        principalTable: "ExternalComponentAssets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalComponentImports_ExternalComponentAssets_FootprintRenderAssetId",
                        column: x => x.FootprintRenderAssetId,
                        principalSchema: "dbo",
                        principalTable: "ExternalComponentAssets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalComponentImports_ExternalComponentAssets_ManualAssetId",
                        column: x => x.ManualAssetId,
                        principalSchema: "dbo",
                        principalTable: "ExternalComponentAssets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalComponentImports_ExternalComponentAssets_StepAssetId",
                        column: x => x.StepAssetId,
                        principalSchema: "dbo",
                        principalTable: "ExternalComponentAssets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalComponentImports_OnlineCandidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "OnlineCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "ExternalImportSources",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Enabled", "IsDeleted", "Notes", "SourceName", "SourceType", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1L, new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Utc), "system", true, false, "Seeded import source for the EasyEDA Pro extension staging connector.", "EasyEDA Pro", 1, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentAssets_AssetType",
                schema: "dbo",
                table: "ExternalComponentAssets",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentAssets_ExternalComponentImportId",
                schema: "dbo",
                table: "ExternalComponentAssets",
                column: "ExternalComponentImportId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentAssets_SourceName_ExternalUuid",
                schema: "dbo",
                table: "ExternalComponentAssets",
                columns: new[] { "SourceName", "ExternalUuid" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_CandidateId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_DatasheetAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "DatasheetAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_FootprintRenderAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "FootprintRenderAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_FootprintUuid",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "FootprintUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_ImportStatus",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "ImportStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_ManualAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "ManualAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_Manufacturer_ManufacturerPN",
                schema: "dbo",
                table: "ExternalComponentImports",
                columns: new[] { "Manufacturer", "ManufacturerPN" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_Model3DUuid",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "Model3DUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_SourceName_ExternalDeviceUuid",
                schema: "dbo",
                table: "ExternalComponentImports",
                columns: new[] { "SourceName", "ExternalDeviceUuid" },
                unique: true,
                filter: "[ExternalDeviceUuid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_SourceName_LcscId",
                schema: "dbo",
                table: "ExternalComponentImports",
                columns: new[] { "SourceName", "LcscId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_StepAssetId",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "StepAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalComponentImports_SymbolUuid",
                schema: "dbo",
                table: "ExternalComponentImports",
                column: "SymbolUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalImportSources_SourceName",
                schema: "dbo",
                table: "ExternalImportSources",
                column: "SourceName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalComponentAssets_ExternalComponentImports_ExternalComponentImportId",
                schema: "dbo",
                table: "ExternalComponentAssets",
                column: "ExternalComponentImportId",
                principalSchema: "dbo",
                principalTable: "ExternalComponentImports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalComponentAssets_ExternalComponentImports_ExternalComponentImportId",
                schema: "dbo",
                table: "ExternalComponentAssets");

            migrationBuilder.DropTable(
                name: "ExternalImportSources",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExternalComponentImports",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExternalComponentAssets",
                schema: "dbo");
        }
    }
}
