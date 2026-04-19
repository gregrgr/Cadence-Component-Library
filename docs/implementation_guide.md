# CadenceCIS v1.0

## Current Deployment

- SQL Server target: `127.0.0.1:1443`
- Database name: `CadenceCIS`
- Auth used for deployment: SQL login `sa`
- Deployment status: schema, release views, and sample seed data created

## Core Objects

- Release view for Capture CIS: `dbo.vw_CIS_Release_Parts`
- Alternate lookup view: `dbo.vw_CIS_Alternates`
- Core master data: `CompanyPart`, `ManufacturerPart`, `PackageFamily`, `FootprintVariant`
- Governance tables: `PartApproval`, `PartDoc`
- Online staging table: `stg_online_part`

## Capture CIS Mapping

Use `dbo.vw_CIS_Release_Parts` as the publication source.

Recommended field mapping:

- Part Number Field: `COMPANY_PN`
- Schematic Part Field: `SCHEMATIC_PART`
- Schematic Library Field: `SCHEMATIC_LIBRARY`
- PCB Footprint Field: `PCB_FOOTPRINT`

Recommended instance properties:

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

## Allegro Library Paths

Recommended shared paths:

- `\\LIB\\Cadence\\Symbols_OLB`
- `\\LIB\\Cadence\\Footprints`
- `\\LIB\\Cadence\\Padstacks`
- `\\LIB\\Cadence\\3D`
- `\\LIB\\Cadence\\Docs`

Recommended Allegro preferences:

- `psmpath=\\LIB\\Cadence\\Footprints`
- `padpath=\\LIB\\Cadence\\Padstacks;\\LIB\\Cadence\\Footprints`
- `steppath=\\LIB\\Cadence\\3D`

## Review Gates

Data review:

- `CompanyPN` unique
- `Manufacturer + ManufacturerPN` unique
- normalized `ValueNorm`
- valid `DatasheetURL`
- lifecycle not obsolete before release
- only `APPROVED` parts published

Symbol review:

- pin number matches datasheet
- pin name clear and consistent
- hidden pins follow team policy
- multi-gate partitioning validated

Footprint review:

- pin 1 orientation correct
- pad and pitch match datasheet
- silkscreen does not violate pad clearance
- courtyard and assembly outlines checked
- STEP alignment verified when present

## Online Ingestion Flow

1. Search candidate MPN using Component Explorer, CIP, or approved web providers.
2. Write raw metadata into `dbo.stg_online_part`.
3. Normalize package geometry and compare against `PackageSignature`.
4. Reuse existing `PackageFamily` when signature matches.
5. Create new `FootprintVariant` only when a new land-pattern variant is required.
6. Promote to `CompanyPart` and `ManufacturerPart` after librarian approval.
7. Publish only through `dbo.vw_CIS_Release_Parts`.

## Acceptance Checks

1. ODBC DSN connects to SQL Server.
2. Capture CIS can browse `dbo.vw_CIS_Release_Parts`.
3. Only `APPROVED` parts appear.
4. `PCB_FOOTPRINT` resolves in Allegro `psmpath`.
5. Required padstacks resolve in `padpath`.
6. Sample parts transfer from schematic to PCB without missing footprint errors.
7. Live BOM can identify `MANUFACTURER_NAME` and `MANUFACTURER_PART_NUMBER`.
