# Phase 1 Plan — Access Request Hub

## Problem understanding

Internal application-access requests need one auditable source of truth. The MVP must prevent duplicate submission, enforce the Manager → System Owner approval workflow, reject unauthorized actions on the server, and preserve an audit trail when two approvers act at nearly the same time.

## Architecture and data model

The .NET 8 project is layered: Models define data, Data contains EF Core/PostgreSQL setup and migrations, Repositories contain persistence queries, RequestService owns business rules and transactions, and Controllers handle HTTP. A Razor MVC `HomeController` serves the frontend, while API controllers expose the required workflow endpoints using the simulated `x-user-email` header.

`AccessRequest.Id`, `Application.Id`, and `AuditEvent.Id` are UUID primary keys. `ClientRequestId` has a unique index for idempotency. `ApplicationId`, `RequesterEmail`, and `AuditEvent.RequestId` are foreign keys. PostgreSQL `xmin` is mapped as the optimistic-concurrency `Version` token.

## Implementation order

1. Verify entities, context, migrations, and seed data.
2. Implement repository queries and the service workflow.
3. Add idempotency, audit transactions, and concurrency handling.
4. Add create, inbox, detail, audit, approve, and reject UI flows.
5. Add automated tests and verify them.
6. Review configuration/security and document evidence.

## Test strategy

The suite covers standard and high-risk approval, forbidden approval, idempotent retry, and stale-version conflict. PostgreSQL migration/seed and browser demo are manual verification steps because EF Core InMemory does not emulate PostgreSQL `xmin`.

## Trade-offs

1. Header-based seeded-user authentication is suitable only for this local MVP.
2. PostgreSQL `xmin` provides provider-level concurrency protection but couples this implementation to PostgreSQL.
3. Razor MVC with small browser-side JavaScript meets the UI scope without a separate Node build pipeline.

## Changes during implementation

- The API-only scope expanded after reviewing the full brief: a frontend, tests, and evidence documents were required.
- Detail access was narrowed to the requester, relevant manager/system owner, or auditor.
- Idempotency was strengthened from a pre-insert lookup to a unique database constraint plus duplicate-key recovery.
