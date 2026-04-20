using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadenceComponentLibraryAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryReleases",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReleaseName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReleaseNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PartCount = table.Column<int>(type: "int", nullable: true),
                    FootprintCount = table.Column<int>(type: "int", nullable: true),
                    SymbolCount = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryReleases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnlineCandidates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceProvider = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ManufacturerPN = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RawPackageName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeadCount = table.Column<int>(type: "int", nullable: true),
                    PitchMm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    BodyLmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    BodyWmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    EPLmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    EPWmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    DatasheetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoHS = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LifecycleStatus = table.Column<int>(type: "int", nullable: false),
                    SymbolDownloaded = table.Column<bool>(type: "bit", nullable: false),
                    FootprintDownloaded = table.Column<bool>(type: "bit", nullable: false),
                    StepDownloaded = table.Column<bool>(type: "bit", nullable: false),
                    CandidateStatus = table.Column<int>(type: "int", nullable: false),
                    ImportNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineCandidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageFamilies",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageFamilyCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeadCount = table.Column<int>(type: "int", nullable: false),
                    BodyLmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    BodyWmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    PitchMm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    EPLmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    EPWmm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    DensityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PackageStd = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PackageSignature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageFamilies", x => x.Id);
                    table.UniqueConstraint("AK_PackageFamilies_PackageFamilyCode", x => x.PackageFamilyCode);
                });

            migrationBuilder.CreateTable(
                name: "PartAlternates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceCompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TargetCompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AltLevel = table.Column<int>(type: "int", nullable: false),
                    SameFootprintYN = table.Column<bool>(type: "bit", nullable: false),
                    SameSymbolYN = table.Column<bool>(type: "bit", nullable: false),
                    NeedEEReviewYN = table.Column<bool>(type: "bit", nullable: false),
                    NeedLayoutReviewYN = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartAlternates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartChangeLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SymbolFamilies",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SymbolFamilyCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    OlbPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PartClass = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GateStyle = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PinMapHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolFamilies", x => x.Id);
                    table.UniqueConstraint("AK_SymbolFamilies_SymbolFamilyCode", x => x.SymbolFamilyCode);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "dbo",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootprintVariants",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FootprintName = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    PackageFamilyCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PsmPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DraPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PadstackSet = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    StepPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VariantType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootprintVariants", x => x.Id);
                    table.UniqueConstraint("AK_FootprintVariants_FootprintName", x => x.FootprintName);
                    table.ForeignKey(
                        name: "FK_FootprintVariants_PackageFamilies_PackageFamilyCode",
                        column: x => x.PackageFamilyCode,
                        principalSchema: "dbo",
                        principalTable: "PackageFamilies",
                        principalColumn: "PackageFamilyCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyParts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PartClass = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ValueNorm = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SymbolFamilyCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PackageFamilyCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DefaultFootprintName = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    LifecycleStatus = table.Column<int>(type: "int", nullable: false),
                    AltGroup = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PreferredYN = table.Column<bool>(type: "bit", nullable: false),
                    HeightMaxMm = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    TempRange = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    RoHS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    REACHStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DatasheetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyParts", x => x.Id);
                    table.UniqueConstraint("AK_CompanyParts_CompanyPN", x => x.CompanyPN);
                    table.ForeignKey(
                        name: "FK_CompanyParts_FootprintVariants_DefaultFootprintName",
                        column: x => x.DefaultFootprintName,
                        principalSchema: "dbo",
                        principalTable: "FootprintVariants",
                        principalColumn: "FootprintName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyParts_PackageFamilies_PackageFamilyCode",
                        column: x => x.PackageFamilyCode,
                        principalSchema: "dbo",
                        principalTable: "PackageFamilies",
                        principalColumn: "PackageFamilyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyParts_SymbolFamilies_SymbolFamilyCode",
                        column: x => x.SymbolFamilyCode,
                        principalSchema: "dbo",
                        principalTable: "SymbolFamilies",
                        principalColumn: "SymbolFamilyCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturerParts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ManufacturerPN = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MfgDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PackageCodeRaw = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SourceProvider = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LifecycleStatus = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    ParamJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturerParts_CompanyParts_CompanyPN",
                        column: x => x.CompanyPN,
                        principalSchema: "dbo",
                        principalTable: "CompanyParts",
                        principalColumn: "CompanyPN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartDocs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyPN = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DocUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LocalPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VersionTag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceProvider = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartDocs_CompanyParts_CompanyPN",
                        column: x => x.CompanyPN,
                        principalSchema: "dbo",
                        principalTable: "CompanyParts",
                        principalColumn: "CompanyPN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierOffers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerPartId = table.Column<long>(type: "bigint", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SupplierSku = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Moq = table.Column<int>(type: "int", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    StockQty = table.Column<long>(type: "bigint", nullable: true),
                    SnapshotAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierOffers_ManufacturerParts_ManufacturerPartId",
                        column: x => x.ManufacturerPartId,
                        principalSchema: "dbo",
                        principalTable: "ManufacturerParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "dbo",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "dbo",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "dbo",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "dbo",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "dbo",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "dbo",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "dbo",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParts_ApprovalStatus",
                schema: "dbo",
                table: "CompanyParts",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParts_CompanyPN",
                schema: "dbo",
                table: "CompanyParts",
                column: "CompanyPN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParts_DefaultFootprintName",
                schema: "dbo",
                table: "CompanyParts",
                column: "DefaultFootprintName");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParts_PackageFamilyCode",
                schema: "dbo",
                table: "CompanyParts",
                column: "PackageFamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParts_SymbolFamilyCode",
                schema: "dbo",
                table: "CompanyParts",
                column: "SymbolFamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_FootprintVariants_FootprintName",
                schema: "dbo",
                table: "FootprintVariants",
                column: "FootprintName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootprintVariants_PackageFamilyCode",
                schema: "dbo",
                table: "FootprintVariants",
                column: "PackageFamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerParts_CompanyPN",
                schema: "dbo",
                table: "ManufacturerParts",
                column: "CompanyPN");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerParts_Manufacturer_ManufacturerPN",
                schema: "dbo",
                table: "ManufacturerParts",
                columns: new[] { "Manufacturer", "ManufacturerPN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineCandidates_Manufacturer_ManufacturerPN",
                schema: "dbo",
                table: "OnlineCandidates",
                columns: new[] { "Manufacturer", "ManufacturerPN" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageFamilies_PackageFamilyCode",
                schema: "dbo",
                table: "PackageFamilies",
                column: "PackageFamilyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageFamilies_PackageSignature",
                schema: "dbo",
                table: "PackageFamilies",
                column: "PackageSignature",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartDocs_CompanyPN",
                schema: "dbo",
                table: "PartDocs",
                column: "CompanyPN");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOffers_ManufacturerPartId",
                schema: "dbo",
                table: "SupplierOffers",
                column: "ManufacturerPartId");

            migrationBuilder.CreateIndex(
                name: "IX_SymbolFamilies_SymbolFamilyCode",
                schema: "dbo",
                table: "SymbolFamilies",
                column: "SymbolFamilyCode",
                unique: true);

            migrationBuilder.Sql(
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
                """);

            migrationBuilder.Sql(
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
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_CIS_Alternates;");

            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_CIS_Release_Parts;");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LibraryReleases",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OnlineCandidates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartAlternates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartChangeLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartDocs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SupplierOffers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ManufacturerParts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CompanyParts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FootprintVariants",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SymbolFamilies",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PackageFamilies",
                schema: "dbo");
        }
    }
}
