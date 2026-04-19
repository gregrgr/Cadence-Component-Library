# Desktop Symbol Library Ingest

Source root:

- `C:\Users\Administrator\Desktop\Symbol_Lib`

Target root:

- `E:\candence-sql\library\Cadence\Symbols_OLB`

## Category Mapping

| Source Folder | Target Folder |
| --- | --- |
| `Active_Components` | `Semiconductor` |
| `Audio_Components_Library` | `Audio` |
| `Board_Mounting_Housing_Archive` | `Mechanical` |
| `Communication_Components_Library` | `Interface` |
| `Connector_Library` | `Connector` |
| `Detection_Tools` | `Protection` |
| `Indicators_and_Displays_Library` | `Display` |
| `Passive_Components` | `Passive` |
| `Power_Components_Library` | `Power` |
| `Sensor_Library` | `Sensor` |
| `Switch_Library` | `Switch_Relay` |

## Current Verified Power Mapping

The desktop power library `Voltage_Regulators.OLB` exposes the visible model name:

- `LDO_5P_1`

Because of that, the current sample `LDO_5PIN` database mapping should use:

- schematic library: `E:\candence-sql\library\Cadence\Symbols_OLB\Power\Voltage_Regulators.OLB`
- schematic part: `LDO_5P_1`
