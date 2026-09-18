# Plan

Phase 1 is a single .NET 8 vertical slice: EF Core/PostgreSQL entities and migrations, repositories for persistence, services for workflow rules and transactions, HTTP controllers, and a lightweight static frontend.

Implementation order: validate schema/seeding, harden service rules and concurrency/idempotency, add UI, add tests, then document verification.

Tests target standard and high-risk approval, forbidden actions, duplicate client IDs, and stale versions. The important trade-offs are simulated header authentication (appropriate only for this local MVP), PostgreSQL `xmin` as the concurrency token, and a small vanilla-JS UI rather than a separate frontend build system.
