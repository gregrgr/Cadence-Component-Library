# Passive Symbol Model Mapping

This file separates the internal symbol family identifier from the actual Capture part name used during placement.

## Recommended Mapping

| Symbol Family | Part Class | Schematic Library | Schematic Part |
| --- | --- | --- | --- |
| `RES_2PIN` | `R` | `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Resistors.OLB` | `RES` |
| `CAP_2PIN` | `C` | `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB` | `CAP_NP` |
| `CAP_POL_2PIN` | `C` | `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Capacitors.OLB` | `CAP` |
| `IND_2PIN` | `L` | `E:\candence-sql\library\Cadence\Symbols_OLB\Passive\Inductors.OLB` | `Inductor` |

## Why This Mapping Exists

- `SymbolFamily` is the enterprise-facing grouping key used in the database.
- `SymbolName` is the real Capture model name that must exist in the `.olb` file.
- For passive parts, the database should not guess the model name from the family name.

## Current Verified Names Found in `passive.olb`

- `RES`
- `CAP`
- `CAP_NP`
- `Inductor`

## Practical Rule

When adding a new passive part:

1. choose the correct `SymbolFamily`
2. let the `SymbolFamily` table map that family to the real `SymbolName`
3. do not write guessed names such as `RES_2PIN` or `CAP_POL_2PIN` into the placed schematic model field unless they are confirmed to exist inside the library
