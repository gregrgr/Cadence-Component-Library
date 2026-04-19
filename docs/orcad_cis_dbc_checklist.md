# OrCAD Capture CIS `.dbc` Configuration Checklist

## Target Environment

- SQL Server endpoint: `127.0.0.1:1443`
- Database: `CadenceCIS`
- Preferred publication view: `dbo.vw_CIS_Release_Parts`
- Alternate view: `dbo.vw_CIS_Alternates`
- Windows DSN name: `CadenceCIS_SQL`

## Goal

Configure OrCAD Capture CIS so designers can:

- browse only approved parts
- place parts from `dbo.vw_CIS_Release_Parts`
- carry `COMPANY_PN`, `MANUFACTURER_PART_NUMBER`, and `PCB_FOOTPRINT` into the schematic
- support downstream Part Manager replacement and Live BOM matching

## Before You Start

Confirm these items first:

1. SQL Server is reachable at `127.0.0.1:1443`.
2. Database `CadenceCIS` exists.
3. View `dbo.vw_CIS_Release_Parts` returns rows.
4. Shared symbol libraries and Allegro footprint paths are already agreed internally.
5. You have a usable ODBC DSN or a direct ODBC login path from Capture.
6. `SCHEMATIC_LIBRARY` returns a valid library file name and the matching `.olb` is already added to the configured libraries list.

## Capture Configuration Path

In OrCAD Capture / OrCAD X Capture CIS:

1. Open `Options > CIS Configuration`.
2. Click `New`.
3. Choose ODBC-based configuration.
4. Select DSN `CadenceCIS_SQL`.
5. Enter SQL login if the DSN does not store credentials.
6. Select table/view `dbo.vw_CIS_Release_Parts`.
7. Save the configuration as a `.dbc` file in your team config location.

Suggested `.dbc` storage location:

- `E:\\candence-sql\\library\\Cadence\\Config\\CadenceCIS_Release.dbc`

## Core Field Mapping

Map these CIS fields exactly:

- Part Number Field: `COMPANY_PN`
- Schematic Part Field: `SCHEMATIC_PART`
- Schematic Library Field: `SCHEMATIC_LIBRARY`
- PCB Footprint Field: `PCB_FOOTPRINT`
- Description Field: `DESCRIPTION`
- Value Field: `VALUE`

Recommended searchable display fields:

- `COMPANY_PN`
- `DESCRIPTION`
- `VALUE`
- `MANUFACTURER_NAME`
- `MANUFACTURER_PART_NUMBER`
- `PACKAGE_FAMILY`
- `LIFECYCLE_STATUS`
- `ROHS`

## Recommended Instance Properties

Push these properties onto the schematic instance:

- `COMPANY_PN`
- `DESCRIPTION`
- `VALUE`
- `MANUFACTURER_NAME`
- `MANUFACTURER_PART_NUMBER`
- `PCB_FOOTPRINT`
- `ALT_GROUP`
- `LIFECYCLE_STATUS`
- `ROHS`
- `DATASHEET_URL`

These are the most important checks:

- `MANUFACTURER_NAME` must be present for Live BOM matching.
- `MANUFACTURER_PART_NUMBER` must be present for Live BOM matching.
- `PCB_FOOTPRINT` must exactly match the `.psm` package symbol name used by Allegro.

## Expected Source Columns

The release view is expected to expose these columns:

- `COMPANY_PN`
- `PART_CLASS`
- `DESCRIPTION`
- `VALUE`
- `MANUFACTURER_NAME`
- `MANUFACTURER_PART_NUMBER`
- `SCHEMATIC_PART`
- `SCHEMATIC_LIBRARY`
- `PCB_FOOTPRINT`
- `PACKAGE_FAMILY`
- `ALT_GROUP`
- `APPROVAL_STATUS`
- `LIFECYCLE_STATUS`
- `ROHS`
- `REACH`
- `HEIGHT_MAX_MM`
- `TEMP_RANGE`
- `DATASHEET_URL`
- `STEP_MODEL`
- `FOOTPRINT_STATUS`

## Placement Smoke Test

Run this exact sequence after saving the `.dbc`:

1. Open Capture.
2. Open the CIS Explorer / database part placement dialog.
3. Search `CPN-R-000001`.
4. Confirm the record appears with description and MPN.
5. Place the part into a test schematic.
6. Open instance properties.
7. Confirm `COMPANY_PN`, `MANUFACTURER_PART_NUMBER`, and `PCB_FOOTPRINT` were written correctly.
8. Repeat for `CPN-C-000001` and `CPN-IC-000001`.

## Part Manager Replacement Test

After the placement smoke test:

1. Open the schematic in Part Manager.
2. Select a placed part with `ALT_GROUP`.
3. Right-click and choose `Link Database Part`.
4. Open the CIS browser.
5. Search using the same `ALT_GROUP` or `COMPANY_PN`.
6. Confirm the replacement candidate keeps the same `PCB_FOOTPRINT` when expected.
7. Apply the link and verify the updated instance properties.

## PCB Hand-Off Test

Validate one full Capture-to-Allegro chain:

1. Place `CPN-R-000001` in Capture.
2. Netlist or sync into PCB.
3. Confirm `PCB_FOOTPRINT = R_0603_1608Metric`.
4. Confirm Allegro resolves the package symbol from `psmpath`.
5. Confirm required padstacks resolve from `padpath`.
6. Confirm there is no missing package symbol error.

## Common Failure Modes

### 1. Part appears in SQL but not in Capture CIS

Check:

- the `.dbc` points to `dbo.vw_CIS_Release_Parts`
- the row has `ApprovalStatus = APPROVED`
- the part is not `OBSOLETE` or `EOL`
- the linked manufacturer part has `IsApproved = 1`
- the footprint variant has `Status = Released`

### 2. Capture finds the row but cannot place the symbol

Check:

- `SCHEMATIC_LIBRARY` file name is valid
- the matching `.olb` is added to the configured libraries list
- `SCHEMATIC_PART` exists in the target `.olb`
- the library is readable from the user machine

### 3. Capture places the symbol but PCB cannot find the footprint

Check:

- `PCB_FOOTPRINT` exactly matches the `.psm` name
- `psmpath` includes the footprint folder
- `padpath` includes every required padstack location

### 4. Live BOM cannot identify the component

Check:

- `MANUFACTURER_NAME` is present on the placed instance
- `MANUFACTURER_PART_NUMBER` is present on the placed instance
- the final schematic instance values were not overwritten manually

### 5. DSN connects inconsistently

Check:

- whether Capture behaves differently from PowerShell ODBC on the same machine
- whether the DSN uses driver 17 or 18
- whether the local workstation requires a different SQL Server ODBC encryption mode

## Useful SQL Checks

Use these when debugging release visibility:

```sql
SELECT TOP 20 *
FROM dbo.vw_CIS_Release_Parts
ORDER BY COMPANY_PN, MANUFACTURER_PART_NUMBER;
```

```sql
SELECT *
FROM dbo.vw_CIS_ReleaseHealthCheck;
```

```sql
SELECT *
FROM dbo.vw_CIS_Alternates
WHERE ALT_GROUP = 'ALT-R-10K-0603';
```

## Recommended Rollout Order

1. Configure the `.dbc` against `dbo.vw_CIS_Release_Parts`.
2. Validate the three seeded sample parts.
3. Validate one replacement flow through Part Manager.
4. Validate one schematic-to-PCB transfer.
5. Freeze the `.dbc` as the team baseline.
6. Start bulk-importing approved parts.
