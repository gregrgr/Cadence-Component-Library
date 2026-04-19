# OLB Folder Strategy

The schematic library area now uses category folders so manufacturer-specific symbol libraries can be added without turning the root directory into a long flat list.

Root:

- `E:\candence-sql\library\Cadence\Symbols_OLB`

Category folders:

- `Audio`
- `Connector`
- `Display`
- `Interface`
- `Mechanical`
- `Passive`
- `Power`
- `Protection`
- `Semiconductor`
- `Sensor`
- `Switch_Relay`

## Practical Rule

- keep the current root-level working files for compatibility with existing Capture configuration
- place new category libraries into the matching folder
- when adding vendor-specific models, use names such as:
  - `Passive\Passive_Murata.olb`
  - `Power\Power_TI.olb`
  - `Interface\Interface_FTDI.olb`
  - `Semiconductor\Semiconductor_Infineon.olb`

## Current Safe Transition Strategy

- root-level `.olb` files remain available for the current working database configuration
- category folders are ready for future library growth
- database and `.dbc` do not need to change immediately
