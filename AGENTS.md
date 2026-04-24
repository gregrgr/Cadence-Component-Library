# AGENTS.md

This repository is `CadenceComponentLibraryAdmin`, an `ASP.NET Core MVC` / `EF Core` / `SQL Server` application for managing `Cadence` / `OrCAD` component-library data, librarian review workflows, and CIS-facing release views.

This file defines repository-wide engineering rules for agents and contributors.

## Core principles

- Preserve the existing layered architecture.
- Prefer small, testable, explicit services over large multi-purpose classes.
- Preserve workflow visibility, reviewability, and auditability.
- Treat imports, AI extraction, and generated library artifacts as staging or review data until explicitly approved.
- Favor additive, low-risk changes over large rewrites.

## Required architecture

Keep the existing structure:

- `src/CadenceComponentLibraryAdmin.Web`
- `src/CadenceComponentLibraryAdmin.Application`
- `src/CadenceComponentLibraryAdmin.Domain`
- `src/CadenceComponentLibraryAdmin.Infrastructure`
- `tests/CadenceComponentLibraryAdmin.Tests`

Expected responsibilities:

- `Web`
  - MVC controllers, views, page composition, authorization, view models, request binding
- `Application`
  - use-case orchestration, service interfaces, workflow coordination, integration contracts
- `Domain`
  - entities, enums, business rules, invariants, approval and release semantics
- `Infrastructure`
  - EF Core persistence, file storage, HTTP clients, job queue runners, external-system adapters
- `Tests`
  - unit, integration, workflow, and controller tests with fake/mock external dependencies

Do not:

- move business rules into controllers or Razor views
- bypass `Application` services by adding ad hoc controller logic
- collapse the layered structure into a single project or service bucket

## Workflow, release, and data-state rules

- Do not remove, bypass, or weaken the existing approval workflow.
- Only approved parts may flow into CIS release views.
- Generated library artifacts must remain draft or review status until approved by a librarian.
- AI extraction or normalization results must be reviewable before any Cadence artifact is generated.
- Do not automatically create approved `CompanyParts`.
- Do not automatically create released `FootprintVariants`.
- Do not automatically publish to CIS release views from imported or AI-generated data.
- Keep review, approval, rejection, and release transitions explicit in code and auditable.

## Cadence automation safety

- Do not implement arbitrary `Tcl` or `SKILL` execution from user input.
- Any `Capture Tcl` or `Allegro SKILL` execution must go through a job queue.
- Job execution must use whitelisted actions only.
- Prefer typed job payloads over raw free-form script text.
- Keep generated commands auditable, attributable, and reviewable.
- Do not require real `Cadence Capture` or `Allegro` installations for CI or standard test runs.
- Use fake or mock Cadence job runners in tests.

## AI and external-provider rules

- Keep external AI providers behind interfaces.
- Keep Cadence integrations behind interfaces.
- Keep external import providers behind interfaces where practical.
- Preserve staging-first behavior for external imports and AI-assisted extraction.
- If data is uncertain, preserve raw source data rather than inventing normalized values.
- Any AI-assisted extraction or classification result should be reviewable in the application before downstream artifact generation.

## MCP tooling rules

- Prefer MCP-backed tooling for environment and repository operations.
- Environment setup, service orchestration, container/runtime interaction, and local stack operations should use `MCP Docker` as the default path.
- GitHub repository, PR, issue, review, and merge operations should use `MCP GitHub` as the default path.
- Do not introduce workflows that depend on local developer-specific GUI setup when an MCP Docker path already exists.

## Database and migration rules

- Preserve the formal EF Core migration baseline.
- Do not modify `InitialCreate` unless absolutely necessary and explicitly justified.
- Add new migrations for schema changes instead of rewriting migration history.
- Keep SQL Server as the authoritative relational target.
- Preserve CIS release view behavior so only approved data is exposed.

## Testing expectations

- Add or update tests for new business logic, workflow transitions, and integration boundaries.
- Prefer deterministic tests with fake time, fake runners, and fake external providers where practical.
- Do not require live Cadence tools, live EasyEDA editor runtimes, or similar heavyweight dependencies in CI.
- If a `.NET SDK` is available, run:
  - `dotnet build`
  - `dotnet test`

## Documentation expectations

- Add or update documentation for every new subsystem.
- Update `README.md` when operator-visible setup, workflows, or architecture expectations change.
- Update subsystem docs under `docs/` when behavior, constraints, or integration models change.
- Keep deprecated paths clearly marked as deprecated rather than silently leaving stale instructions in place.

## Security and review rules

- Do not expose hidden state transitions that skip librarian or reviewer oversight.
- Do not store secrets in source control.
- Do not collect or persist credentials, cookies, session tokens, or browser-local secrets from external tools unless the repository explicitly implements a reviewed and approved auth model.
- Keep authentication and authorization checks server-side even when the UI hides unavailable actions.

## Change guidance

- Prefer minimal, explicit changes with clear ownership.
- Preserve existing approval, audit, and release semantics.
- Keep new subsystems behind interfaces and covered by tests.
- Avoid introducing hidden automation that changes part state without user-visible review steps.
- After each completed operation or logical work unit, create a local commit.
- Push commits to the remote after every 10 local commits, or earlier when explicitly requested or when a PR/review handoff is needed.
