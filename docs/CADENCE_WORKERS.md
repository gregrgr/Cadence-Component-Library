# Cadence Workers

This document describes the file-based Cadence worker bridge introduced in Milestone 3.

## Purpose

The bridge adds deterministic queue files and worker-script templates for future Cadence automation without requiring real Cadence installations in CI.

It does not execute real Capture or Allegro automation in this milestone.

## Worker environments

- the Capture worker runs inside the OrCAD Capture Tcl environment
- the Allegro worker runs inside the Allegro SKILL environment

Both workers consume deterministic JSON job files from queue folders.

## Queue layout

Capture queue:

- `storage/jobs/capture/pending`
- `storage/jobs/capture/running`
- `storage/jobs/capture/done`
- `storage/jobs/capture/failed`

Allegro queue:

- `storage/jobs/allegro/pending`
- `storage/jobs/allegro/running`
- `storage/jobs/allegro/done`
- `storage/jobs/allegro/failed`

## Job lifecycle

1. the application creates a `CadenceBuildJob`
2. a bridge service writes deterministic job JSON into `pending`
3. a worker moves the job to `running`
4. the worker writes deterministic result JSON into `done` or `failed`
5. the application records `CadenceBuildArtifact` rows and hashes artifacts where applicable

## Allowed actions

Capture:

- `create_symbol`
- `verify_symbol`

Allegro:

- `create_footprint`
- `verify_footprint`

Unknown actions must be rejected.

Raw `Tcl` and raw `SKILL` execution from queue input are forbidden.

## Library root configuration

Configuration keys:

- `CadenceAutomation:JobRoot`
- `CadenceAutomation:CaptureQueuePath`
- `CadenceAutomation:AllegroQueuePath`
- `CadenceAutomation:LibraryRoot`

The worker should resolve generated draft outputs beneath the configured `LibraryRoot`.

## Artifact recording

Generated artifacts such as `OLB`, `PAD`, `DRA`, `PSM`, `STEP`, reports, previews, and JSON outputs are recorded as `CadenceBuildArtifact` rows.

When files exist on disk:

- the bridge computes `SHA256`
- the path is persisted
- the artifact remains associated with the originating `CadenceBuildJob`

## Development simulator

In `Development`, Admin and Librarian users can use the Jobs page simulator to complete a pending job without launching Cadence tools.

The simulator:

- moves the queue file through the same `pending` -> `running` -> `done` lifecycle
- writes a simulation report artifact under `CadenceAutomation:LibraryRoot/_simulated-workers`
- records the report as a `CadenceBuildArtifact`
- computes `SHA256` through the same artifact hashing path
- never executes Capture Tcl or Allegro SKILL
- never creates approved parts or publishes CIS release data

This is a local validation aid only. Production Cadence output must still come from reviewed worker scripts running inside the appropriate Cadence environment.

## Development verification reports

In `Development`, Admin and Librarian users can generate a verification report from the current job and artifact state after symbol and footprint jobs succeed.

The development report:

- writes a `LibraryVerificationReport`
- records symbol and footprint report JSON
- reports `Pass` only when both Capture symbol and Allegro footprint jobs have succeeded
- includes recorded artifact paths and hashes
- explicitly marks the report as simulated
- never creates approved `CompanyPart` rows
- never publishes to CIS release views

Production verification should be generated from real Capture and Allegro worker results, not from the development simulator.

## Safety rules

- no raw script execution from user input
- only whitelisted actions
- deterministic JSON in and JSON out
- default overwrite policy is `fail_if_exists`
- released artifacts must not be overwritten
- CI tests must run without installed Cadence software
