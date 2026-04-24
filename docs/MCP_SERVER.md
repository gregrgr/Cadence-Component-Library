# MCP Server

This document describes the skeleton MCP server for AI-assisted Cadence library workflow.

## Scope of Milestone 2

Milestone 2 adds:

- a new console host project:
  - `src/CadenceComponentLibraryAdmin.Mcp`
- MCP-oriented tool semantics for AI extraction review and Cadence job queue orchestration
- a placeholder MCP server adapter so the project remains buildable even when the official MCP C# SDK is not restored in the current environment

Milestone 2 does not add:

- direct OpenAI calls
- direct Cadence Capture execution
- direct Allegro execution
- raw `Tcl` or raw `SKILL` execution from tool input

## Why there is a placeholder adapter

The repository targets `.NET 10`, but this environment cannot currently validate restoring the official Model Context Protocol C# SDK.

To keep the project buildable:

- tool logic is implemented in `IMcpLibraryWorkflowService`
- the console project uses a `PlaceholderMcpServerAdapter`
- once the official MCP SDK is restored and validated, the adapter can be replaced without rewriting workflow logic

## How to run

If the `.NET SDK` is available:

```powershell
dotnet run --project src/CadenceComponentLibraryAdmin.Mcp
```

Current behavior:

- loads configuration
- wires EF Core and workflow services
- starts the placeholder MCP adapter
- logs the available tool names

## Configuration

The MCP host reads:

- `ConnectionStrings:DefaultConnection`
- `CadenceAutomation:JobRoot`
- `CadenceAutomation:CaptureQueuePath`
- `CadenceAutomation:AllegroQueuePath`
- `CadenceAutomation:LibraryRoot`

Example values live in:

- `src/CadenceComponentLibraryAdmin.Mcp/appsettings.json`

## Tool names

The current catalog exposes these tool semantics:

- `library_get_candidate`
- `library_search_duplicate`
- `datasheet_create_extraction_draft`
- `datasheet_submit_for_review`
- `datasheet_approve_for_build`
- `capture_enqueue_symbol_job`
- `allegro_enqueue_footprint_job`
- `cadence_get_job_status`
- `verification_get_report`

## Example tool calls

Example: get candidate summary

```json
{
  "tool": "library_get_candidate",
  "input": {
    "candidateId": 42
  }
}
```

Example: search duplicates

```json
{
  "tool": "library_search_duplicate",
  "input": {
    "manufacturer": "Texas Instruments",
    "manufacturerPartNumber": "SN74LVC1G14DBVR",
    "packageName": "SOT-23-5"
  }
}
```

Example: create extraction draft

```json
{
  "tool": "datasheet_create_extraction_draft",
  "input": {
    "candidateId": 42,
    "extractionJson": "{\"summary\":\"draft\"}",
    "symbolSpecJson": "{\"pins\":[]}",
    "footprintSpecJson": "{\"pads\":[]}"
  }
}
```

Example: enqueue a Capture symbol job

```json
{
  "tool": "capture_enqueue_symbol_job",
  "input": {
    "extractionId": 1001
  }
}
```

## Security boundaries

- `datasheet_approve_for_build` should only be treated as authoritative when an authenticated `Admin` or `Librarian` context exists
- until MCP auth is added, the Web application remains the authoritative approval boundary
- MCP tool input must not be treated as permission to execute arbitrary automation
- all Cadence automation must stay queued, typed, and auditable

## Why raw Tcl and SKILL are forbidden

Raw `Tcl` and `SKILL` are forbidden because they create unsafe, non-reviewable execution paths.

Required pattern:

- only whitelisted queued actions
- typed job payloads
- persisted `InputJson`
- persisted status and output
- auditable artifact records

That keeps AI-assisted workflow reviewable and prevents arbitrary user input from becoming direct Capture or Allegro execution.
