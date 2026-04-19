# ODBC Notes

## Current State

- A Windows user DSN named `CadenceCIS_SQL` has been created for the local workstation.
- Target server is `127.0.0.1,1443`.
- Target database is `CadenceCIS`.

## Important Compatibility Note

This machine's installed `.NET ODBC` path showed a TLS/encryption handshake error with `ODBC Driver 17 for SQL Server` when tested from PowerShell, even though direct `SqlClient` connections to the same Docker SQL Server succeed.

Observed symptom:

- `Encryption not supported on the client`
- `No credentials are available in the security package`

## Practical Recommendation

In OrCAD Capture CIS, test the DSN directly from the CIS configuration dialog first.

If the DSN still fails inside Capture:

1. Edit the DSN to use the legacy `SQL Server` ODBC driver if your Capture environment accepts it.
2. Keep the SQL Server endpoint on local TCP `127.0.0.1,1443`.
3. Prefer local non-TLS connection for this workstation-to-local-Docker setup.
4. If needed, switch to a SQL Server ODBC driver build that is known-good with your Windows image.

## Connection Values

- DSN name: `CadenceCIS_SQL`
- Server: `127.0.0.1,1443`
- Database: `CadenceCIS`
- Login: `sa`

## Capture CIS Mapping Reminder

- Table/View: `dbo.vw_CIS_Release_Parts`
- Part Number Field: `COMPANY_PN`
- Schematic Part Field: `SCHEMATIC_PART`
- Schematic Library Field: `SCHEMATIC_LIBRARY`
- PCB Footprint Field: `PCB_FOOTPRINT`
