# Allegro SKILL Worker

This folder contains queue-worker templates for running inside the Allegro SKILL environment.

## Purpose

The worker reads deterministic job JSON files from the Allegro queue and performs only whitelisted actions.

Allowed actions:

- `create_footprint`
- `verify_footprint`

Raw SKILL from user input is forbidden.

## Expected job JSON shape

```json
{
  "jobId": 124,
  "queueFamily": "allegro",
  "action": "create_footprint",
  "overwritePolicy": "fail_if_exists",
  "candidateId": 42,
  "aiDatasheetExtractionId": 1001,
  "manufacturer": "Texas Instruments",
  "manufacturerPartNumber": "SN74LVC1G14DBVR",
  "libraryRoot": "library/Cadence",
  "specJson": "{...footprint spec json...}",
  "resultJsonPath": "storage/jobs/allegro/done/124.result.json",
  "requestedByTool": "AllegroQueue",
  "requestedAtUtc": "2026-04-24T09:00:00.0000000Z"
}
```

## Expected result JSON shape

```json
{
  "jobId": 124,
  "status": "Succeeded",
  "action": "create_footprint",
  "messages": [],
  "artifacts": [
    {
      "artifactType": "DRA",
      "filePath": "library/Cadence/Footprints/MyPart.dra"
    },
    {
      "artifactType": "PSM",
      "filePath": "library/Cadence/Footprints/MyPart.psm"
    }
  ]
}
```

## Safety rules

- reject unknown actions
- reject overwrite of released artifacts
- honor `fail_if_exists` by default
- write deterministic result JSON
- never execute raw SKILL from queue input
