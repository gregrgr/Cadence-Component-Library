USE [CadenceCIS];
GO

CREATE OR ALTER VIEW dbo.vw_CIS_Release_Parts AS
SELECT
    CAST(cp.CompanyPN AS VARCHAR(80))              AS COMPANY_PN,
    CAST(
        CASE cp.PartClass
            WHEN N'R' THEN N'Resistor'
            WHEN N'C' THEN N'Capacitor'
            WHEN N'L' THEN N'Inductor'
            WHEN N'IC' THEN N'Integrated Circuit'
            WHEN N'CONN' THEN N'Connector'
            WHEN N'D' THEN N'Diode'
            WHEN N'Q' THEN N'Transistor'
            ELSE cp.PartClass
        END
        AS VARCHAR(40)
    )                                              AS PART_CLASS,
    CAST(cp.Description AS VARCHAR(255))           AS DESCRIPTION,
    CAST(cp.ValueNorm AS VARCHAR(80))              AS VALUE,
    CAST(mp.Manufacturer AS VARCHAR(120))          AS MANUFACTURER_NAME,
    CAST(mp.ManufacturerPN AS VARCHAR(160))        AS MANUFACTURER_PART_NUMBER,
    CAST(sf.SymbolName AS VARCHAR(120))            AS SCHEMATIC_PART,
    CAST(sf.OlbPath AS VARCHAR(260))               AS SCHEMATIC_LIBRARY,
    CAST(cp.DefaultFootprint AS VARCHAR(140))      AS PCB_FOOTPRINT,
    CAST(cp.PackageFamily AS VARCHAR(120))         AS PACKAGE_FAMILY,
    CAST(cp.AltGroup AS VARCHAR(80))               AS ALT_GROUP,
    CAST(cp.ApprovalStatus AS VARCHAR(20))         AS APPROVAL_STATUS,
    CAST(cp.LifecycleStatus AS VARCHAR(20))        AS LIFECYCLE_STATUS,
    CAST(cp.RoHS AS VARCHAR(20))                   AS ROHS,
    CAST(cp.REACHStatus AS VARCHAR(20))            AS REACH,
    cp.HeightMaxMM            AS HEIGHT_MAX_MM,
    CAST(cp.TempRange AS VARCHAR(60))              AS TEMP_RANGE,
    CAST(cp.DatasheetURL AS VARCHAR(500))          AS DATASHEET_URL,
    CAST(fv.StepPath AS VARCHAR(260))              AS STEP_MODEL,
    CAST(fv.Status AS VARCHAR(20))                 AS FOOTPRINT_STATUS
FROM dbo.CompanyPart cp
JOIN dbo.ManufacturerPart mp
  ON mp.CompanyPN = cp.CompanyPN
 AND mp.IsApproved = 1
JOIN dbo.SymbolFamily sf
  ON sf.SymbolFamily = cp.SymbolFamily
 AND sf.IsActive = 1
JOIN dbo.FootprintVariant fv
  ON fv.FootprintName = cp.DefaultFootprint
 AND fv.Status = N'Released'
WHERE cp.ApprovalStatus = N'APPROVED'
  AND ISNULL(cp.LifecycleStatus, N'ACTIVE') NOT IN (N'OBSOLETE', N'EOL');
GO

CREATE OR ALTER VIEW dbo.vw_CIS_Alternates AS
SELECT
    CAST(cp.CompanyPN AS VARCHAR(80))              AS COMPANY_PN,
    CAST(cp.AltGroup AS VARCHAR(80))               AS ALT_GROUP,
    CAST(cp.Description AS VARCHAR(255))           AS DESCRIPTION,
    CAST(cp.ValueNorm AS VARCHAR(80))              AS VALUE,
    CAST(mp.Manufacturer AS VARCHAR(120))          AS MANUFACTURER_NAME,
    CAST(mp.ManufacturerPN AS VARCHAR(160))        AS MANUFACTURER_PART_NUMBER,
    CAST(cp.DefaultFootprint AS VARCHAR(140))      AS PCB_FOOTPRINT,
    CAST(cp.PackageFamily AS VARCHAR(120))         AS PACKAGE_FAMILY,
    CAST(cp.LifecycleStatus AS VARCHAR(20))        AS LIFECYCLE_STATUS,
    CAST(cp.RoHS AS VARCHAR(20))                   AS ROHS
FROM dbo.CompanyPart cp
JOIN dbo.ManufacturerPart mp
  ON mp.CompanyPN = cp.CompanyPN
 AND mp.IsApproved = 1
WHERE cp.ApprovalStatus = N'APPROVED'
  AND cp.AltGroup IS NOT NULL
  AND ISNULL(cp.LifecycleStatus, N'ACTIVE') NOT IN (N'OBSOLETE', N'EOL');
GO

INSERT INTO dbo.SymbolFamily (SymbolFamily, SymbolName, PartClass, OlbPath, GateStyle)
VALUES
(N'RES_2PIN', N'RES', N'R', N'E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB', N'SINGLE'),
(N'CAP_2PIN', N'CAP_NP', N'C', N'E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB', N'SINGLE'),
(N'CAP_POL_2PIN', N'CAP', N'C', N'E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB', N'SINGLE'),
(N'IND_2PIN', N'Inductor', N'L', N'E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB', N'SINGLE'),
(N'LDO_5PIN', N'LDO_5P_1', N'IC', N'E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB', N'SINGLE');
GO

INSERT INTO dbo.PackageFamily (
    PackageFamily, PartClass, MountType, MetricSize, LeadCount, LeadArrangement,
    BodyL_mm, BodyW_mm, Pitch_mm, EP_L_mm, EP_W_mm, DensityLevel, PackageStd, Notes
)
VALUES
(N'R_0603_1608Metric', N'R', N'SMT', N'0603_1608Metric', 2, N'PASSIVE', 1.600, 0.800, NULL, NULL, NULL, N'IPC-B', N'IPC-7351', N'Common resistor 0603 metric'),
(N'C_0603_1608Metric', N'C', N'SMT', N'0603_1608Metric', 2, N'PASSIVE', 1.600, 0.800, NULL, NULL, NULL, N'IPC-B', N'IPC-7351', N'Common capacitor 0603 metric'),
(N'SOT23_5', N'IC', N'SMT', NULL, 5, N'SOT23', 2.900, 1.600, 0.950, NULL, NULL, N'IPC-B', N'JEDEC', N'Generic SOT23-5 family');
GO

INSERT INTO dbo.FootprintVariant (
    FootprintName, PackageFamily, PsmPath, DraPath, PadstackSet, StepPath,
    VariantType, CourtyardRule, AssemblyRule, Status
)
VALUES
(N'R_0603_1608Metric', N'R_0603_1608Metric', N'E:\candence-sql\library\Cadence\Footprints\R_0603_1608Metric.psm', N'E:\candence-sql\library\Cadence\Footprints\R_0603_1608Metric.dra', N'SMT_0603', N'E:\candence-sql\library\Cadence\3D\R_0603.step', N'IPC-B', N'IPC-B', N'STD', N'Released'),
(N'C_0603_1608Metric', N'C_0603_1608Metric', N'E:\candence-sql\library\Cadence\Footprints\C_0603_1608Metric.psm', N'E:\candence-sql\library\Cadence\Footprints\C_0603_1608Metric.dra', N'SMT_0603', N'E:\candence-sql\library\Cadence\3D\C_0603.step', N'IPC-B', N'IPC-B', N'STD', N'Released'),
(N'SOT23_5', N'SOT23_5', N'E:\candence-sql\library\Cadence\Footprints\SOT23_5.psm', N'E:\candence-sql\library\Cadence\Footprints\SOT23_5.dra', N'SOT23_PADSET', N'E:\candence-sql\library\Cadence\3D\SOT23_5.step', N'IPC-B', N'IPC-B', N'STD', N'Released');
GO

INSERT INTO dbo.CompanyPart (
    CompanyPN, PartClass, Description, ValueNorm, SymbolFamily, PackageFamily,
    DefaultFootprint, ApprovalStatus, AltGroup, PreferredYN, MountType,
    HeightMaxMM, TempRange, RoHS, REACHStatus, LifecycleStatus,
    DatasheetURL, CreatedBy, UpdatedBy
)
VALUES
(N'CPN-R-000001', N'R', N'RES 10K 1% 0603', N'10k', N'RES_2PIN', N'R_0603_1608Metric', N'R_0603_1608Metric', N'APPROVED', N'ALT-R-10K-0603', 1, N'SMT', 0.550, N'-55~155C', N'Compliant', N'Compliant', N'ACTIVE', N'https://example.com/datasheet-r-10k.pdf', N'codex', N'codex'),
(N'CPN-C-000001', N'C', N'CAP 100nF 10% 0603 X7R 50V', N'100nF', N'CAP_2PIN', N'C_0603_1608Metric', N'C_0603_1608Metric', N'APPROVED', N'ALT-C-100N-0603', 1, N'SMT', 0.900, N'-55~125C', N'Compliant', N'Compliant', N'ACTIVE', N'https://example.com/datasheet-c-100nf.pdf', N'codex', N'codex'),
(N'CPN-IC-000001', N'IC', N'LDO 3.3V 500mA SOT23-5', N'3.3V', N'LDO_5PIN', N'SOT23_5', N'SOT23_5', N'APPROVED', N'ALT-LDO-3V3-500MA', 1, N'SMT', 1.450, N'-40~125C', N'Compliant', N'Compliant', N'ACTIVE', N'https://example.com/datasheet-ldo-3v3.pdf', N'codex', N'codex');
GO

INSERT INTO dbo.ManufacturerPart (
    CompanyPN, Manufacturer, ManufacturerPN, MfgDescription, PackageCodeRaw,
    SourceProvider, LifecycleStatus, VerifiedBy, VerifiedAt, IsApproved
)
VALUES
(N'CPN-R-000001', N'YAGEO', N'RC0603FR-0710KL', N'RES SMD 10K 1% 0603', N'0603', N'Web/CIP', N'ACTIVE', N'codex', SYSDATETIME(), 1),
(N'CPN-R-000001', N'Panasonic', N'ERJ-3EKF1002V', N'RES SMD 10K 1% 0603', N'0603', N'Web/CIP', N'ACTIVE', N'codex', SYSDATETIME(), 1),
(N'CPN-C-000001', N'Murata', N'GRM188R71H104KA93D', N'CAP 100nF 50V X7R 0603', N'0603', N'Web/CIP', N'ACTIVE', N'codex', SYSDATETIME(), 1),
(N'CPN-IC-000001', N'Microchip', N'MIC5205-3.3YM5', N'LDO 3.3V SOT23-5', N'SOT23-5', N'Component Explorer', N'ACTIVE', N'codex', SYSDATETIME(), 1);
GO

INSERT INTO dbo.PartApproval (
    CompanyPN, ApprovalStage, ApprovalStatus, Reviewer, ReviewComment, ReviewedAt
)
VALUES
(N'CPN-R-000001', N'LIBRARY_REVIEW', N'APPROVED', N'codex', N'Initial sample record approved for CIS integration test.', SYSDATETIME()),
(N'CPN-C-000001', N'LIBRARY_REVIEW', N'APPROVED', N'codex', N'Initial sample record approved for CIS integration test.', SYSDATETIME()),
(N'CPN-IC-000001', N'LIBRARY_REVIEW', N'APPROVED', N'codex', N'Initial sample record approved for CIS integration test.', SYSDATETIME());
GO
