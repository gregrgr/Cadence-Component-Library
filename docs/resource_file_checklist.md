# Resource File Checklist

This checklist is derived from the current sample records in the local `CadenceCIS` database.

## Schematic Symbol Libraries

Place these files under `E:\candence-sql\library\Cadence\Symbols_OLB`:

- `passive.olb`
- `power_ldo_dcdc.olb`

Database storage rule for schematic libraries:

- Store only the library file name in `SCHEMATIC_LIBRARY`
- Example:
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB`

Capture configuration rule:

- Add the actual local `.olb` files to the configured libraries list
- Example:
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB`

## Printed Circuit Board Footprint Files

Place these files under `E:\candence-sql\library\Cadence\Footprints`:

- `R_0603_1608Metric.psm`
- `R_0603_1608Metric.dra`
- `C_0603_1608Metric.psm`
- `C_0603_1608Metric.dra`
- `SOT23_5.psm`
- `SOT23_5.dra`

## Padstack Files

Place these files under `E:\candence-sql\library\Cadence\Padstacks` unless your team keeps them next to footprint files:

- `SMT_0603.pad`
- `SOT23_PADSET.pad`

## Three-Dimensional Model Files

Place these files under `E:\candence-sql\library\Cadence\3D`:

- `R_0603.step`
- `C_0603.step`
- `SOT23_5.step`

## Database Rows Currently Bound to These Resources

- `CPN-R-000001`
  - schematic symbol library: `passive.olb`
  - printed circuit board footprint: `R_0603_1608Metric`
- `CPN-C-000001`
  - schematic symbol library: `passive.olb`
  - printed circuit board footprint: `C_0603_1608Metric`
- `CPN-IC-000001`
  - schematic symbol library: `power_ldo_dcdc.olb`
  - printed circuit board footprint: `SOT23_5`

## Final Verification

1. Open Capture and load `E:\candence-sql\library\Cadence\Config\CadenceCIS_Release.dbc`.
2. Place `CPN-R-000001`.
3. Confirm the symbol resolves from `passive.olb`.
4. Transfer to the printed circuit board editor.
5. Confirm `R_0603_1608Metric.psm` resolves from the local footprint directory.
6. Confirm required padstack files resolve from the local padstack search path.
