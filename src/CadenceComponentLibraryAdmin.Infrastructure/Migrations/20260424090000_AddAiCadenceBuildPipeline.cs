using CadenceComponentLibraryAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260424090000_AddAiCadenceBuildPipeline")]
    public partial class AddAiCadenceBuildPipeline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiDatasheetExtractions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<long>(type: "bigint", nullable: true),
                    ExternalImportId = table.Column<long>(type: "bigint", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DatasheetAssetPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExtractionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SymbolSpecJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FootprintSpecJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDatasheetExtractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiDatasheetExtractions_ExternalComponentImports_ExternalImportId",
                        column: x => x.ExternalImportId,
                        principalSchema: "dbo",
                        principalTable: "ExternalComponentImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDatasheetExtractions_OnlineCandidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "OnlineCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AiExtractionEvidence",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AiDatasheetExtractionId = table.Column<long>(type: "bigint", nullable: false),
                    FieldPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ValueText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SourcePage = table.Column<int>(type: "int", nullable: true),
                    SourceTable = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SourceFigure = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ReviewerDecision = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReviewerNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiExtractionEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiExtractionEvidence_AiDatasheetExtractions_AiDatasheetExtractionId",
                        column: x => x.AiDatasheetExtractionId,
                        principalSchema: "dbo",
                        principalTable: "AiDatasheetExtractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CadenceBuildJobs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<long>(type: "bigint", nullable: true),
                    AiDatasheetExtractionId = table.Column<long>(type: "bigint", nullable: true),
                    InputJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ToolName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ToolVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadenceBuildJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CadenceBuildJobs_AiDatasheetExtractions_AiDatasheetExtractionId",
                        column: x => x.AiDatasheetExtractionId,
                        principalSchema: "dbo",
                        principalTable: "AiDatasheetExtractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CadenceBuildJobs_OnlineCandidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "OnlineCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LibraryVerificationReports",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<long>(type: "bigint", nullable: true),
                    CompanyPartId = table.Column<long>(type: "bigint", nullable: true),
                    AiDatasheetExtractionId = table.Column<long>(type: "bigint", nullable: true),
                    SymbolReportJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FootprintReportJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryVerificationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryVerificationReports_AiDatasheetExtractions_AiDatasheetExtractionId",
                        column: x => x.AiDatasheetExtractionId,
                        principalSchema: "dbo",
                        principalTable: "AiDatasheetExtractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LibraryVerificationReports_CompanyParts_CompanyPartId",
                        column: x => x.CompanyPartId,
                        principalSchema: "dbo",
                        principalTable: "CompanyParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LibraryVerificationReports_OnlineCandidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "OnlineCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CadenceBuildArtifacts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CadenceBuildJobId = table.Column<long>(type: "bigint", nullable: false),
                    ArtifactType = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadenceBuildArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CadenceBuildArtifacts_CadenceBuildJobs_CadenceBuildJobId",
                        column: x => x.CadenceBuildJobId,
                        principalSchema: "dbo",
                        principalTable: "CadenceBuildJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiDatasheetExtractions_CandidateId",
                schema: "dbo",
                table: "AiDatasheetExtractions",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDatasheetExtractions_ExternalImportId",
                schema: "dbo",
                table: "AiDatasheetExtractions",
                column: "ExternalImportId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDatasheetExtractions_Manufacturer_ManufacturerPartNumber",
                schema: "dbo",
                table: "AiDatasheetExtractions",
                columns: new[] { "Manufacturer", "ManufacturerPartNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AiDatasheetExtractions_Status",
                schema: "dbo",
                table: "AiDatasheetExtractions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AiExtractionEvidence_AiDatasheetExtractionId",
                schema: "dbo",
                table: "AiExtractionEvidence",
                column: "AiDatasheetExtractionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiExtractionEvidence_AiDatasheetExtractionId_FieldPath",
                schema: "dbo",
                table: "AiExtractionEvidence",
                columns: new[] { "AiDatasheetExtractionId", "FieldPath" });

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildArtifacts_ArtifactType",
                schema: "dbo",
                table: "CadenceBuildArtifacts",
                column: "ArtifactType");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildArtifacts_CadenceBuildJobId",
                schema: "dbo",
                table: "CadenceBuildArtifacts",
                column: "CadenceBuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildJobs_AiDatasheetExtractionId",
                schema: "dbo",
                table: "CadenceBuildJobs",
                column: "AiDatasheetExtractionId");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildJobs_CandidateId",
                schema: "dbo",
                table: "CadenceBuildJobs",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildJobs_JobType",
                schema: "dbo",
                table: "CadenceBuildJobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceBuildJobs_Status",
                schema: "dbo",
                table: "CadenceBuildJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryVerificationReports_AiDatasheetExtractionId",
                schema: "dbo",
                table: "LibraryVerificationReports",
                column: "AiDatasheetExtractionId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryVerificationReports_CandidateId",
                schema: "dbo",
                table: "LibraryVerificationReports",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryVerificationReports_CompanyPartId",
                schema: "dbo",
                table: "LibraryVerificationReports",
                column: "CompanyPartId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryVerificationReports_OverallStatus",
                schema: "dbo",
                table: "LibraryVerificationReports",
                column: "OverallStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiExtractionEvidence",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CadenceBuildArtifacts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LibraryVerificationReports",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CadenceBuildJobs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AiDatasheetExtractions",
                schema: "dbo");
        }
    }
}
