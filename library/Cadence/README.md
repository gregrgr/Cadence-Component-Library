# Cadence Resource Library

This directory is the local resource root used by the sample `CadenceCIS` configuration.

Expected subdirectories:

- `Config`
- `Symbols_OLB`
- `Footprints`
- `Padstacks`
- `3D`
- `Docs`

Current sample database rows and `.dbc`-driven placement tests should reference this local root instead of the placeholder network path `\\LIB\\Cadence`.

Recommended usage:

- Put `.dbc` files in `Config`
- Put `.olb` symbol libraries in `Symbols_OLB`
- Put `.psm` and `.dra` files in `Footprints`
- Put `.pad` files in `Padstacks`
- Put `.step` files in `3D`
- Put datasheets and related documents in `Docs`
