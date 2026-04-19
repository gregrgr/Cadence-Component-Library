# Resource Directory Layout

Local resource root:

- `E:\candence-sql\library\Cadence`

Subdirectories:

- `E:\candence-sql\library\Cadence\Config`
- `E:\candence-sql\library\Cadence\Symbols_OLB`
- `E:\candence-sql\library\Cadence\Footprints`
- `E:\candence-sql\library\Cadence\Padstacks`
- `E:\candence-sql\library\Cadence\3D`
- `E:\candence-sql\library\Cadence\Docs`

Current sample configuration alignment:

- `.dbc` file:
  - `E:\candence-sql\library\Cadence\Config\CadenceCIS_Release.dbc`
- symbol library values in `dbo.SymbolFamily.OlbPath`:
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB`
- footprint paths in `dbo.FootprintVariant`:
  - `E:\candence-sql\library\Cadence\Footprints\*.psm`
  - `E:\candence-sql\library\Cadence\Footprints\*.dra`
- three-dimensional model paths in `dbo.FootprintVariant.StepPath`:
  - `E:\candence-sql\library\Cadence\3D\*.step`

Recommended Allegro search path values:

- `psmpath = E:\candence-sql\library\Cadence\Footprints`
- `padpath = E:\candence-sql\library\Cadence\Padstacks;E:\candence-sql\library\Cadence\Footprints`
- `steppath = E:\candence-sql\library\Cadence\3D`

Notes:

- The directories now exist locally, but the actual `.olb`, `.psm`, `.dra`, `.pad`, and `.step` production files still need to be placed into them.
- The sample database now uses a mixed strategy:
  - `SCHEMATIC_LIBRARY` stores relative library file names only.
  - `PCB_FOOTPRINT` stores package symbol names only.
  - Capture placement still requires the matching `.olb` files to be present in the configured libraries list.
  - footprint and three-dimensional model resolution rely on tool search paths.

Current sample file names expected under the local resource root:

- schematic symbol libraries:
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB`
  - `E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB`
- package symbol files:
  - `R_0603_1608Metric.psm`
  - `R_0603_1608Metric.dra`
  - `C_0603_1608Metric.psm`
  - `C_0603_1608Metric.dra`
  - `SOT23_5.psm`
  - `SOT23_5.dra`
- padstack files:
  - `SMT_0603.pad`
  - `SOT23_PADSET.pad`
- three-dimensional model files:
  - `R_0603.step`
  - `C_0603.step`
  - `SOT23_5.step`

Related local configuration files:

- `E:\candence-sql\library\Cadence\Config\CadenceCIS_Release.dbc`
- `E:\candence-sql\config\allegro_local_paths.txt`
- `E:\candence-sql\config\capture_cis_local_paths.txt`

Practical completion criteria for this resource tree:

1. All required `.olb` files exist in `Symbols_OLB`.
2. All required `.psm` and `.dra` files exist in `Footprints`.
3. All required `.pad` files exist in `Padstacks` or `Footprints`.
4. All required `.step` files exist in `3D`.
5. Capture can place a database part without a missing symbol error.
6. Allegro can resolve the package symbol and padstack without a missing library error.
