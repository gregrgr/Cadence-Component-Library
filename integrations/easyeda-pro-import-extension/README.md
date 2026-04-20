# EasyEDA Pro Import Extension

Deprecated: Milestone B4 replaces this extension-driven flow with the backend EasyEDA/LCSC nlbn-style importer. This folder is retained only as historical reference and is not the recommended or CI-validated path.

This extension exports EasyEDA Pro library metadata into the staging layer of `CadenceComponentLibraryAdmin`.

Important architecture rule:

- The EasyEDA Pro SDK runs inside EasyEDA Pro.
- The `.NET` backend does not call `easyeda/pro-api-sdk` directly.
- The extension collects SDK data, preserves raw payloads, and POSTs them into the backend import APIs.

## Build

Requirements:

- Node.js `>= 20.17.0`

Commands:

```bash
npm install
npm run typecheck
npm run build
```

Build output:

- bundled extension code: `dist/`
- installable package: `build/dist/easyeda-pro-import-extension_v0.1.0.eext`

## Install into EasyEDA Pro

1. Build the extension with `npm run build`.
2. Open EasyEDA Pro.
3. Use the extension installation flow in EasyEDA Pro to install the generated `.eext` package from `build/dist/`.

The exact extension-install entry point may vary by EasyEDA Pro version, so follow the current EasyEDA Pro extension-install UX for local packages.

## Configure

Use the extension menu:

- `CIS Import -> Configure Connector...`

Settings:

- `backendBaseUrl`
  - example: `http://localhost:8080`
- `importApiKey`
  - must match backend `ExternalImports:EasyEdaApiKey`
- `defaultLibraryUuid`
  - optional

The extension stores these settings locally in browser storage for the EasyEDA Pro runtime.

## CI-safe validation

This project is designed so CI can validate the extension bundle without needing the EasyEDA Pro editor runtime:

- `npm run typecheck`
  - checks TypeScript shape and imports
- `npm run sanity-check`
  - runs pure helper assertions for document-link extraction and payload normalization
- `npm run build`
  - runs typecheck, sanity-check, bundle, and package generation

Runtime-only behavior:

- direct `eda.*` API calls still require the EasyEDA Pro editor environment
- CI does not attempt to execute EasyEDA editor APIs outside EasyEDA Pro

## Import modes

Available menu actions:

- `Export Component to CIS Admin...`
  - keyword-based search through `LIB_Device.search`
- `Export by LCSC C-number...`
  - direct lookup through `LIB_Device.getByLcscIds`
- `Export Current Selection (best effort)`
  - placeholder entry for runtimes where selected-library-device APIs are available

## Backend API expectations

The backend must expose:

- `POST /api/import/easyeda/component`
- `POST /api/import/easyeda/component/{id}/asset`

Security:

- the extension sends `X-Import-Api-Key`
- anonymous import without the API key is not supported

## What gets preserved

The extension sends both normalized fields and raw JSON snapshots:

- search item JSON
- device item JSON
- association JSON
- property JSON
- otherProperty JSON
- full merged raw JSON

If the runtime exposes a footprint render image or document URL, the extension uploads it as an asset or preserves the source URL.

## Known limitations

- EasyEDA library APIs used by this extension are BETA.
- `getByLcscIds` may be unavailable in private deployments.
- `getRenderImage` is called only when the runtime exposes it.
- Direct STEP binary download is not guaranteed; URL preservation is used when necessary.
- Imported EasyEDA footprints do not become Allegro `PSM` / `DRA` files automatically.
