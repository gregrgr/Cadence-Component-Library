USE [CadenceCIS];
GO

CREATE OR ALTER VIEW dbo.vw_CIS_PackageReuseCandidates AS
SELECT
    s.StgID,
    s.SourceProvider,
    s.Manufacturer,
    s.ManufacturerPN,
    s.Description,
    s.RawPackageName,
    s.MountType,
    s.PartClass,
    s.MetricSize,
    s.LeadCount,
    s.LeadArrangement,
    s.PitchMM,
    s.BodyLMM,
    s.BodyWMM,
    s.EPLMM,
    s.EPWMM,
    pf.PackageFamily,
    pf.PackageSignature,
    CASE
        WHEN pf.PackageFamily IS NOT NULL THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS ReuseRecommended
FROM dbo.stg_online_part s
LEFT JOIN dbo.PackageFamily pf
    ON pf.PackageSignature = CONCAT(
        ISNULL(s.MountType, N'NA'), N'|',
        ISNULL(s.PartClass, N'NA'), N'|',
        ISNULL(s.MetricSize, N'NA'), N'|',
        ISNULL(CONVERT(NVARCHAR(16), s.LeadCount), N'NA'), N'|',
        ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), s.BodyLMM)), N'NA'), N'|',
        ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), s.BodyWMM)), N'NA'), N'|',
        ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), s.PitchMM)), N'NA'), N'|',
        ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), s.EPLMM)), N'0.000'), N'|',
        ISNULL(CONVERT(NVARCHAR(20), CONVERT(DECIMAL(8,3), s.EPWMM)), N'0.000'), N'|',
        ISNULL(s.LeadArrangement, N'NA')
    );
GO

CREATE OR ALTER VIEW dbo.vw_CIS_ReleaseHealthCheck AS
SELECT
    cp.CompanyPN,
    cp.Description,
    cp.ApprovalStatus,
    cp.DefaultFootprint,
    cp.SymbolFamily,
    cp.PackageFamily,
    CASE WHEN sf.SymbolFamily IS NULL THEN N'MISSING_SYMBOL_FAMILY' END AS SymbolIssue,
    CASE WHEN fv.FootprintName IS NULL THEN N'MISSING_FOOTPRINT' END AS FootprintIssue,
    CASE WHEN ap.ApprovedCount = 0 THEN N'NO_APPROVED_MPN' END AS MpnIssue
FROM dbo.CompanyPart cp
LEFT JOIN dbo.SymbolFamily sf
    ON sf.SymbolFamily = cp.SymbolFamily
   AND sf.IsActive = 1
LEFT JOIN dbo.FootprintVariant fv
    ON fv.FootprintName = cp.DefaultFootprint
   AND fv.Status = N'Released'
LEFT JOIN (
    SELECT CompanyPN, COUNT(*) AS ApprovedCount
    FROM dbo.ManufacturerPart
    WHERE IsApproved = 1
    GROUP BY CompanyPN
) ap
    ON ap.CompanyPN = cp.CompanyPN
WHERE cp.ApprovalStatus = N'APPROVED'
  AND (
      sf.SymbolFamily IS NULL
      OR fv.FootprintName IS NULL
      OR ISNULL(ap.ApprovedCount, 0) = 0
  );
GO

CREATE OR ALTER PROCEDURE dbo.usp_CIS_StageOnlinePart
    @SourceProvider      NVARCHAR(60),
    @Manufacturer        NVARCHAR(120),
    @ManufacturerPN      NVARCHAR(160),
    @Description         NVARCHAR(255) = NULL,
    @RawPackageName      NVARCHAR(120) = NULL,
    @MountType           NVARCHAR(20) = NULL,
    @PartClass           NVARCHAR(40) = NULL,
    @MetricSize          NVARCHAR(40) = NULL,
    @LeadCount           INT = NULL,
    @LeadArrangement     NVARCHAR(40) = NULL,
    @PitchMM             DECIMAL(8,3) = NULL,
    @BodyLMM             DECIMAL(8,3) = NULL,
    @BodyWMM             DECIMAL(8,3) = NULL,
    @EPLMM               DECIMAL(8,3) = NULL,
    @EPWMM               DECIMAL(8,3) = NULL,
    @DatasheetURL        NVARCHAR(500) = NULL,
    @RoHS                NVARCHAR(30) = NULL,
    @LifecycleStatus     NVARCHAR(30) = NULL,
    @ImportNote          NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.stg_online_part (
        SourceProvider, Manufacturer, ManufacturerPN, Description, RawPackageName,
        MountType, PartClass, MetricSize, LeadCount, LeadArrangement,
        PitchMM, BodyLMM, BodyWMM, EPLMM, EPWMM,
        DatasheetURL, RoHS, LifecycleStatus, ImportNote
    )
    VALUES (
        @SourceProvider, @Manufacturer, @ManufacturerPN, @Description, @RawPackageName,
        @MountType, @PartClass, @MetricSize, @LeadCount, @LeadArrangement,
        @PitchMM, @BodyLMM, @BodyWMM, @EPLMM, @EPWMM,
        @DatasheetURL, @RoHS, @LifecycleStatus, @ImportNote
    );

    SELECT SCOPE_IDENTITY() AS StgID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CIS_FindPackageMatch
    @StgID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.vw_CIS_PackageReuseCandidates
    WHERE StgID = @StgID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CIS_PromoteApprovedPart
    @CompanyPN             NVARCHAR(80),
    @PartClass             NVARCHAR(40),
    @Description           NVARCHAR(255),
    @ValueNorm             NVARCHAR(80) = NULL,
    @SymbolFamily          NVARCHAR(80),
    @PackageFamily         NVARCHAR(120),
    @DefaultFootprint      NVARCHAR(140),
    @Manufacturer          NVARCHAR(120),
    @ManufacturerPN        NVARCHAR(160),
    @MfgDescription        NVARCHAR(255) = NULL,
    @PackageCodeRaw        NVARCHAR(120) = NULL,
    @SourceProvider        NVARCHAR(40) = NULL,
    @AltGroup              NVARCHAR(80) = NULL,
    @MountType             NVARCHAR(20) = NULL,
    @HeightMaxMM           DECIMAL(8,3) = NULL,
    @TempRange             NVARCHAR(60) = NULL,
    @RoHS                  NVARCHAR(20) = NULL,
    @REACHStatus           NVARCHAR(20) = NULL,
    @LifecycleStatus       NVARCHAR(20) = NULL,
    @DatasheetURL          NVARCHAR(500) = NULL,
    @Reviewer              NVARCHAR(60) = NULL,
    @ReviewComment         NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    MERGE dbo.CompanyPart AS target
    USING (
        SELECT
            @CompanyPN AS CompanyPN,
            @PartClass AS PartClass,
            @Description AS Description,
            @ValueNorm AS ValueNorm,
            @SymbolFamily AS SymbolFamily,
            @PackageFamily AS PackageFamily,
            @DefaultFootprint AS DefaultFootprint,
            @AltGroup AS AltGroup,
            @MountType AS MountType,
            @HeightMaxMM AS HeightMaxMM,
            @TempRange AS TempRange,
            @RoHS AS RoHS,
            @REACHStatus AS REACHStatus,
            @LifecycleStatus AS LifecycleStatus,
            @DatasheetURL AS DatasheetURL
    ) AS src
    ON target.CompanyPN = src.CompanyPN
    WHEN MATCHED THEN
        UPDATE SET
            PartClass = src.PartClass,
            Description = src.Description,
            ValueNorm = src.ValueNorm,
            SymbolFamily = src.SymbolFamily,
            PackageFamily = src.PackageFamily,
            DefaultFootprint = src.DefaultFootprint,
            ApprovalStatus = N'APPROVED',
            AltGroup = src.AltGroup,
            MountType = src.MountType,
            HeightMaxMM = src.HeightMaxMM,
            TempRange = src.TempRange,
            RoHS = src.RoHS,
            REACHStatus = src.REACHStatus,
            LifecycleStatus = src.LifecycleStatus,
            DatasheetURL = src.DatasheetURL,
            UpdatedBy = ISNULL(@Reviewer, SUSER_SNAME()),
            UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (
            CompanyPN, PartClass, Description, ValueNorm, SymbolFamily, PackageFamily,
            DefaultFootprint, ApprovalStatus, AltGroup, MountType, HeightMaxMM,
            TempRange, RoHS, REACHStatus, LifecycleStatus, DatasheetURL,
            CreatedBy, UpdatedBy
        )
        VALUES (
            src.CompanyPN, src.PartClass, src.Description, src.ValueNorm, src.SymbolFamily, src.PackageFamily,
            src.DefaultFootprint, N'APPROVED', src.AltGroup, src.MountType, src.HeightMaxMM,
            src.TempRange, src.RoHS, src.REACHStatus, src.LifecycleStatus, src.DatasheetURL,
            ISNULL(@Reviewer, SUSER_SNAME()), ISNULL(@Reviewer, SUSER_SNAME())
        );

    MERGE dbo.ManufacturerPart AS target
    USING (
        SELECT
            @CompanyPN AS CompanyPN,
            @Manufacturer AS Manufacturer,
            @ManufacturerPN AS ManufacturerPN,
            @MfgDescription AS MfgDescription,
            @PackageCodeRaw AS PackageCodeRaw,
            @SourceProvider AS SourceProvider,
            @LifecycleStatus AS LifecycleStatus
    ) AS src
    ON target.Manufacturer = src.Manufacturer
   AND target.ManufacturerPN = src.ManufacturerPN
    WHEN MATCHED THEN
        UPDATE SET
            CompanyPN = src.CompanyPN,
            MfgDescription = src.MfgDescription,
            PackageCodeRaw = src.PackageCodeRaw,
            SourceProvider = src.SourceProvider,
            LifecycleStatus = src.LifecycleStatus,
            VerifiedBy = ISNULL(@Reviewer, SUSER_SNAME()),
            VerifiedAt = SYSDATETIME(),
            IsApproved = 1
    WHEN NOT MATCHED THEN
        INSERT (
            CompanyPN, Manufacturer, ManufacturerPN, MfgDescription, PackageCodeRaw,
            SourceProvider, LifecycleStatus, VerifiedBy, VerifiedAt, IsApproved
        )
        VALUES (
            src.CompanyPN, src.Manufacturer, src.ManufacturerPN, src.MfgDescription, src.PackageCodeRaw,
            src.SourceProvider, src.LifecycleStatus, ISNULL(@Reviewer, SUSER_SNAME()), SYSDATETIME(), 1
        );

    INSERT INTO dbo.PartApproval (
        CompanyPN, ApprovalStage, ApprovalStatus, Reviewer, ReviewComment, ReviewedAt
    )
    VALUES (
        @CompanyPN, N'LIBRARY_REVIEW', N'APPROVED',
        ISNULL(@Reviewer, SUSER_SNAME()),
        ISNULL(@ReviewComment, N'Approved through usp_CIS_PromoteApprovedPart.'),
        SYSDATETIME()
    );

    COMMIT TRANSACTION;
END;
GO
