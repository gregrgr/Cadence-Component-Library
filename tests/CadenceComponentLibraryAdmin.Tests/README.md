# Tests

This test project now contains real smoke and unit coverage for the current milestone:

- startup hardening around identity seeding
- audit timestamp persistence in `ApplicationDbContext`
- approval and package-signature business rules
- CIS view bootstrap statements

The suite is intentionally lightweight so it can run in CI with `dotnet test` before any Docker or SQL Server based manual validation.

