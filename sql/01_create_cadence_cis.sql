IF DB_ID(N'CadenceCIS') IS NULL
BEGIN
    CREATE DATABASE [CadenceCIS];
END;
GO

USE [CadenceCIS];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.vw_CIS_Alternates', N'V') IS NOT NULL DROP VIEW dbo.vw_CIS_Alternates;
IF OBJECT_ID(N'dbo.vw_CIS_Release_Parts', N'V') IS NOT NULL DROP VIEW dbo.vw_CIS_Release_Parts;
GO

IF OBJECT_ID(N'dbo.SupplierOffer', N'U') IS NOT NULL DROP TABLE dbo.SupplierOffer;
IF OBJECT_ID(N'dbo.PartApproval', N'U') IS NOT NULL DROP TABLE dbo.PartApproval;
IF OBJECT_ID(N'dbo.PartDoc', N'U') IS NOT NULL DROP TABLE dbo.PartDoc;
IF OBJECT_ID(N'dbo.ManufacturerPart', N'U') IS NOT NULL DROP TABLE dbo.ManufacturerPart;
IF OBJECT_ID(N'dbo.CompanyPart', N'U') IS NOT NULL DROP TABLE dbo.CompanyPart;
IF OBJECT_ID(N'dbo.FootprintVariant', N'U') IS NOT NULL DROP TABLE dbo.FootprintVariant;
IF OBJECT_ID(N'dbo.PackageFamily', N'U') IS NOT NULL DROP TABLE dbo.PackageFamily;
IF OBJECT_ID(N'dbo.SymbolFamily', N'U') IS NOT NULL DROP TABLE dbo.SymbolFamily;
IF OBJECT_ID(N'dbo.stg_online_part', N'U') IS NOT NULL DROP TABLE dbo.stg_online_part;
GO

CREATE TABLE dbo.SymbolFamily (
    SymbolFamily       NVARCHAR(80)  NOT NULL PRIMARY KEY,
    SymbolName         NVARCHAR(120) NOT NULL,
    PartClass          NVARCHAR(40)  NOT NULL,
    OlbPath            NVARCHAR(260) NOT NULL,
    GateStyle          NVARCHAR(40)  NULL,
    PinMapHash         NVARCHAR(64)  NULL,
    IsActive           BIT           NOT NULL CONSTRAINT DF_SymbolFamily_IsActive DEFAULT 1,
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_SymbolFamily_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt          DATETIME2     NOT NULL CONSTRAINT DF_SymbolFamily_UpdatedAt DEFAULT SYSDATETIME()
);
GO

CREATE TABLE dbo.PackageFamily (
    PackageFamily      NVARCHAR(120) NOT NULL PRIMARY KEY,
    PartClass          NVARCHAR(40)  NULL,
    MountType          NVARCHAR(20)  NOT NULL,
    MetricSize         NVARCHAR(40)  NULL,
    LeadCount          INT           NOT NULL,
    LeadArrangement    NVARCHAR(40)  NULL,
    BodyL_mm           DECIMAL(8,3)  NULL,
    BodyW_mm           DECIMAL(8,3)  NULL,
    Pitch_mm           DECIMAL(8,3)  NULL,
    EP_L_mm            DECIMAL(8,3)  NULL,
    EP_W_mm            DECIMAL(8,3)  NULL,
    DensityLevel       NVARCHAR(10)  NULL,
    PackageStd         NVARCHAR(40)  NULL,
    Notes              NVARCHAR(500) NULL,
    PackageSignature   AS (
        CONCAT(
            ISNULL(MountType, N'NA'), N'|',
            ISNULL(PartClass, N'NA'), N'|',
            ISNULL(MetricSize, N'NA'), N'|',
            CONVERT(NVARCHAR(16), LeadCount), N'|',
            ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), BodyL_mm)), N'NA'), N'|',
            ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), BodyW_mm)), N'NA'), N'|',
            ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), Pitch_mm)), N'NA'), N'|',
            ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), EP_L_mm)), N'0.000'), N'|',
            ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), EP_W_mm)), N'0.000'), N'|',
            ISNULL(LeadArrangement, N'NA')
        )
    ) PERSISTED
);
GO

CREATE UNIQUE INDEX UX_PackageFamily_Signature
ON dbo.PackageFamily(PackageSignature);
GO

CREATE TABLE dbo.FootprintVariant (
    FootprintName      NVARCHAR(140) NOT NULL PRIMARY KEY,
    PackageFamily      NVARCHAR(120) NOT NULL,
    PsmPath            NVARCHAR(260) NOT NULL,
    DraPath            NVARCHAR(260) NULL,
    PadstackSet        NVARCHAR(120) NULL,
    StepPath           NVARCHAR(260) NULL,
    VariantType        NVARCHAR(30)  NOT NULL,
    CourtyardRule      NVARCHAR(60)  NULL,
    AssemblyRule       NVARCHAR(60)  NULL,
    Status             NVARCHAR(20)  NOT NULL CONSTRAINT DF_FootprintVariant_Status DEFAULT 'Released',
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_FootprintVariant_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt          DATETIME2     NOT NULL CONSTRAINT DF_FootprintVariant_UpdatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_FootprintVariant_PackageFamily FOREIGN KEY (PackageFamily) REFERENCES dbo.PackageFamily(PackageFamily)
);
GO

CREATE TABLE dbo.CompanyPart (
    CompanyPN          NVARCHAR(80)  NOT NULL PRIMARY KEY,
    PartClass          NVARCHAR(40)  NOT NULL,
    Description        NVARCHAR(255) NOT NULL,
    ValueNorm          NVARCHAR(80)  NULL,
    SymbolFamily       NVARCHAR(80)  NOT NULL,
    PackageFamily      NVARCHAR(120) NOT NULL,
    DefaultFootprint   NVARCHAR(140) NOT NULL,
    ApprovalStatus     NVARCHAR(20)  NOT NULL CONSTRAINT DF_CompanyPart_ApprovalStatus DEFAULT 'PENDING',
    AltGroup           NVARCHAR(80)  NULL,
    PreferredYN        BIT           NOT NULL CONSTRAINT DF_CompanyPart_PreferredYN DEFAULT 1,
    MountType          NVARCHAR(20)  NULL,
    HeightMaxMM        DECIMAL(8,3)  NULL,
    TempRange          NVARCHAR(60)  NULL,
    RoHS               NVARCHAR(20)  NULL,
    REACHStatus        NVARCHAR(20)  NULL,
    LifecycleStatus    NVARCHAR(20)  NULL,
    DatasheetURL       NVARCHAR(500) NULL,
    CreatedBy          NVARCHAR(60)  NULL,
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_CompanyPart_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedBy          NVARCHAR(60)  NULL,
    UpdatedAt          DATETIME2     NOT NULL CONSTRAINT DF_CompanyPart_UpdatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CompanyPart_SymbolFamily FOREIGN KEY (SymbolFamily) REFERENCES dbo.SymbolFamily(SymbolFamily),
    CONSTRAINT FK_CompanyPart_PackageFamily FOREIGN KEY (PackageFamily) REFERENCES dbo.PackageFamily(PackageFamily),
    CONSTRAINT FK_CompanyPart_FootprintVariant FOREIGN KEY (DefaultFootprint) REFERENCES dbo.FootprintVariant(FootprintName)
);
GO

CREATE TABLE dbo.ManufacturerPart (
    MpnID              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CompanyPN          NVARCHAR(80)  NOT NULL,
    Manufacturer       NVARCHAR(120) NOT NULL,
    ManufacturerPN     NVARCHAR(160) NOT NULL,
    MfgDescription     NVARCHAR(255) NULL,
    PackageCodeRaw     NVARCHAR(120) NULL,
    SourceProvider     NVARCHAR(40)  NULL,
    LifecycleStatus    NVARCHAR(20)  NULL,
    ParamJSON          NVARCHAR(MAX) NULL,
    VerifiedBy         NVARCHAR(60)  NULL,
    VerifiedAt         DATETIME2     NULL,
    IsApproved         BIT           NOT NULL CONSTRAINT DF_ManufacturerPart_IsApproved DEFAULT 0,
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_ManufacturerPart_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_ManufacturerPart UNIQUE (Manufacturer, ManufacturerPN),
    CONSTRAINT FK_ManufacturerPart_CompanyPart FOREIGN KEY (CompanyPN) REFERENCES dbo.CompanyPart(CompanyPN)
);
GO

CREATE TABLE dbo.SupplierOffer (
    OfferID            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MpnID              BIGINT        NOT NULL,
    SupplierName       NVARCHAR(120) NOT NULL,
    SupplierSKU        NVARCHAR(120) NULL,
    CurrencyCode       NVARCHAR(10)  NULL,
    UnitPrice          DECIMAL(18,6) NULL,
    MOQ                INT           NULL,
    LeadTimeDays       INT           NULL,
    StockQty           BIGINT        NULL,
    SnapshotAt         DATETIME2     NOT NULL CONSTRAINT DF_SupplierOffer_SnapshotAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_SupplierOffer_ManufacturerPart FOREIGN KEY (MpnID) REFERENCES dbo.ManufacturerPart(MpnID)
);
GO

CREATE TABLE dbo.PartDoc (
    DocID              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CompanyPN          NVARCHAR(80)  NOT NULL,
    DocType            NVARCHAR(30)  NOT NULL,
    DocURL             NVARCHAR(500) NULL,
    LocalPath          NVARCHAR(260) NULL,
    VersionTag         NVARCHAR(40)  NULL,
    SourceProvider     NVARCHAR(40)  NULL,
    CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_PartDoc_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_PartDoc_CompanyPart FOREIGN KEY (CompanyPN) REFERENCES dbo.CompanyPart(CompanyPN)
);
GO

CREATE TABLE dbo.PartApproval (
    ApprovalID         BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CompanyPN          NVARCHAR(80)   NOT NULL,
    ApprovalStage      NVARCHAR(30)   NOT NULL,
    ApprovalStatus     NVARCHAR(20)   NOT NULL,
    Reviewer           NVARCHAR(60)   NULL,
    ReviewComment      NVARCHAR(1000) NULL,
    ReviewedAt         DATETIME2      NULL,
    CreatedAt          DATETIME2      NOT NULL CONSTRAINT DF_PartApproval_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_PartApproval_CompanyPart FOREIGN KEY (CompanyPN) REFERENCES dbo.CompanyPart(CompanyPN)
);
GO

CREATE TABLE dbo.stg_online_part (
    StgID                  BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SourceProvider         NVARCHAR(60)  NOT NULL,
    Manufacturer           NVARCHAR(120) NOT NULL,
    ManufacturerPN         NVARCHAR(160) NOT NULL,
    Description            NVARCHAR(255) NULL,
    RawPackageName         NVARCHAR(120) NULL,
    MountType              NVARCHAR(20)  NULL,
    PartClass              NVARCHAR(40)  NULL,
    MetricSize             NVARCHAR(40)  NULL,
    LeadCount              INT           NULL,
    LeadArrangement        NVARCHAR(40)  NULL,
    PitchMM                DECIMAL(8,3)  NULL,
    BodyLMM                DECIMAL(8,3)  NULL,
    BodyWMM                DECIMAL(8,3)  NULL,
    EPLMM                  DECIMAL(8,3)  NULL,
    EPWMM                  DECIMAL(8,3)  NULL,
    DatasheetURL           NVARCHAR(500) NULL,
    RoHS                   NVARCHAR(30)  NULL,
    LifecycleStatus        NVARCHAR(30)  NULL,
    SymbolDownloaded       BIT           NOT NULL CONSTRAINT DF_stg_online_part_SymbolDownloaded DEFAULT 0,
    FootprintDownloaded    BIT           NOT NULL CONSTRAINT DF_stg_online_part_FootprintDownloaded DEFAULT 0,
    StepDownloaded         BIT           NOT NULL CONSTRAINT DF_stg_online_part_StepDownloaded DEFAULT 0,
    CandidateStatus        NVARCHAR(30)  NOT NULL CONSTRAINT DF_stg_online_part_CandidateStatus DEFAULT 'NEW_FROM_WEB',
    ImportNote             NVARCHAR(500) NULL,
    CreatedAt              DATETIME2     NOT NULL CONSTRAINT DF_stg_online_part_CreatedAt DEFAULT SYSDATETIME()
);
GO

CREATE INDEX IX_CompanyPart_Class ON dbo.CompanyPart(PartClass, ApprovalStatus);
CREATE INDEX IX_CompanyPart_Footprint ON dbo.CompanyPart(DefaultFootprint);
CREATE INDEX IX_MfrPart_CompanyPN ON dbo.ManufacturerPart(CompanyPN, IsApproved);
CREATE INDEX IX_Offer_MpnID ON dbo.SupplierOffer(MpnID, SnapshotAt DESC);
CREATE INDEX IX_stg_online_part_Mpn ON dbo.stg_online_part(Manufacturer, ManufacturerPN, CandidateStatus);
GO
