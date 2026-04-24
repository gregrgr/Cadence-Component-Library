# Capture Tcl Worker

This folder contains queue-worker templates for running inside the OrCAD Capture Tcl environment.

## Purpose

The worker reads deterministic job JSON files from the Capture queue and performs only whitelisted actions.

Allowed actions:

- `create_symbol`
- `verify_symbol`

Raw Tcl from user input is forbidden.

## Expected job JSON shape

```json
{
  "jobId": 123,
  "queueFamily": "capture",
  "action": "create_symbol",
  "overwritePolicy": "fail_if_exists",
  "candidateId": 42,
  "aiDatasheetExtractionId": 1001,
  "manufacturer": "Texas Instruments",
  "manufacturerPartNumber": "SN74LVC1G14DBVR",
  "libraryRoot": "library/Cadence",
  "specJson": "{...symbol spec json...}",
  "resultJsonPath": "storage/jobs/capture/done/123.result.json",
  "requestedByTool": "CaptureQueue",
  "requestedAtUtc": "2026-04-24T09:00:00.0000000Z"
}
```

## Expected result JSON shape

```json
{
  "jobId": 123,
  "status": "Succeeded",
  "action": "create_symbol",
  "messages": [],
  "artifacts": [
    {
      "artifactType": "OLB",
      "filePath": "library/Cadence/Symbols/MyPart.olb"
    }
  ]
}
```

## Safety rules

- reject unknown actions
- reject overwrite of released artifacts
- honor `fail_if_exists` by default
- write deterministic result JSON
- never execute raw Tcl from queue input
