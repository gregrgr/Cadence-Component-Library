# AI MCP Cadence Pipeline

This document describes the persistence and review model for AI-assisted library creation in `CadenceComponentLibraryAdmin`.

## Scope of Milestone 1

Milestone 1 adds persistence and review structures only.

It does not:

- call OpenAI
- call Cadence Capture
- call Allegro
- generate real `OLB`, `DRA`, `PSM`, or `PAD` files

The goal is to make AI extraction results reviewable and auditable before any Cadence build automation is introduced.

## Pipeline overview

```mermaid
flowchart LR
    A["Datasheet or staged external import"] --> B["AI extraction result"]
    B --> C["AiDatasheetExtraction"]
    C --> D["AiExtractionEvidence"]
    C --> E["Human review"]
    E --> F["ApprovedForBuild"]
    F --> G["CadenceBuildJob queue"]
    G --> H["CadenceBuildArtifact"]
    G --> I["LibraryVerificationReport"]
```

## AI extraction

`AiDatasheetExtraction` stores the top-level normalized result from a datasheet or staged import.

It persists:

- manufacturer
- manufacturer part number
- extraction JSON
- symbol spec JSON
- footprint spec JSON
- confidence
- review status
- optional link to an `OnlineCandidate`
- optional link to an `ExternalComponentImport`

This model is intentionally staging-only and review-oriented.

## Pluggable extraction services

The extraction pipeline is provider-neutral.

Current interfaces:

- `IDatasheetTextExtractor`
- `IAiDatasheetExtractionService`
- `IJsonSchemaValidationService`

Current implementations:

- `LocalPdfTextExtractor`
  - currently a safe placeholder when no approved PDF text extraction library is wired
- `StubAiDatasheetExtractionService`
  - deterministic development and test implementation
  - produces valid `component_extraction`, `symbol_spec`, and `footprint_spec` JSON
- `CodexCliDatasheetExtractionService`
  - optional local CLI provider
  - disabled by default
  - invokes `codex exec` through a controlled process adapter
  - validates the final structured output before saving
- `OpenAiCompatibleDatasheetExtractionService`
  - optional
  - disabled by default
  - API key must come from configuration or environment
  - API keys must never be logged

All AI output is validated against repository schemas before save.

### Codex CLI provider

The Codex CLI provider is selected with:

```json
{
  "AiExtraction": {
    "Mode": "CodexCli",
    "CodexCli": {
      "Enabled": true,
      "Command": "codex",
      "Model": "",
      "Profile": "",
      "Sandbox": "read-only",
      "Ephemeral": true,
      "TimeoutSeconds": 180,
      "WorkingDirectory": ""
    }
  }
}
```

Runtime behavior:

- the Web application calls `IAiDatasheetExtractionService`
- `CodexCliDatasheetExtractionService` builds a structured extraction prompt
- `CodexCliRunner` invokes `codex exec`
- the CLI final message must contain one JSON object
- the JSON object must include:
  - `componentExtraction`
  - `symbolSpec`
  - `footprintSpec`
  - `confidence`
  - `evidence`
  - `warnings`
- schema validation and critical evidence checks run before the result is saved

The Codex CLI provider does not receive permission to publish library data, run Cadence tools, or execute arbitrary `Tcl` / `SKILL`. It only proposes reviewable JSON that remains in `Draft` or `NeedsReview`.

CI uses fake `ICodexCliRunner` tests and does not require a real Codex CLI login.

## Field-level evidence

`AiExtractionEvidence` stores field-level traceability for extracted values.

Each evidence row can point to:

- a field path
- extracted value text
- optional unit
- source page
- source table
- source figure
- confidence
- reviewer decision and note

This allows a librarian or reviewer to inspect not just the extracted result, but also where it came from.

## Human review

AI extraction is not self-authorizing.

Expected review flow:

1. Extraction enters `Draft` or `NeedsReview`.
2. Evidence rows are reviewed field by field.
3. The extraction is either:
   - kept in review
   - rejected
   - promoted to `ApprovedForBuild`

Approval for build is not the same thing as approval for release.

It only means the extraction is allowed to enter the controlled Cadence build pipeline.

## Capture Tcl job queue

Future Capture automation must use `CadenceBuildJob` records instead of executing raw `Tcl` directly from user input.

Rules:

- no arbitrary raw `Tcl` execution from user input
- only queued jobs may invoke Capture-related actions
- actions must be whitelisted and typed
- inputs must be serialized in `InputJson`
- outputs and machine-readable results must be captured in `OutputJson`

## Allegro SKILL job queue

Future Allegro automation follows the same rule set.

Rules:

- no arbitrary raw `SKILL` execution from user input
- only queued jobs may invoke Allegro-related actions
- only whitelisted actions are allowed
- job inputs and outputs must be persisted for audit and review

## Build jobs and artifacts

`CadenceBuildJob` represents queued or completed work such as:

- `CaptureSymbol`
- `AllegroFootprint`
- `Verification`

`CadenceBuildArtifact` stores outputs linked to a build job, including planned artifact types such as:

- `OLB`
- `DRA`
- `PSM`
- `PAD`
- `STEP`
- `Report`
- `Preview`
- `Json`

Artifacts remain traceable to the job that produced them.

## Verification reports

`LibraryVerificationReport` stores post-build or pre-release verification results.

This can include:

- symbol verification JSON
- footprint verification JSON
- overall pass/warning/fail result

Verification status does not override the librarian approval workflow.

## Approval remains mandatory

Non-negotiable rules:

- AI extraction must be reviewable before Cadence artifact generation
- build jobs must be queued and whitelisted
- generated artifacts remain draft or review data until a librarian approves them
- only approved parts may be published to CIS release views
- this milestone does not alter existing CIS release view behavior
