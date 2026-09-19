# AI Usage Record

This is a working record. Before submission, the candidate must check it reflects their own interactions and decisions.

## 1. Requirements gap analysis

- **Ask:** Compare the existing project with the complete assessment brief.
- **Suggestion:** Add the missing frontend, automated tests, evidence documents, detail authorization, and race-safe idempotency.
- **Decision:** Accepted after review because each item appears in the Phase 1 brief and closes a concrete gap.
- **Why:** The API-generation prompt did not include all PDF requirements.

## 2. Workflow safety review

- **Ask:** Review service-layer workflow safety and concurrency.
- **Suggestion:** Save state and audit changes together, check `xmin`/Version for stale actions, and recover duplicate creation using the unique database constraint.
- **Decision:** Accepted; PostgreSQL-specific duplicate detection was retained deliberately.
- **Why:** A simple application-level pre-check cannot safely handle concurrent submission or approval.

## 3. Frontend scope

- **Ask:** Create a minimal frontend for the critical access-request flow.
- **Suggestion:** Use a Razor MVC view with small browser-side JavaScript instead of a separate SPA build system.
- **Decision:** Accepted after confirming the frontend does not make authorization decisions; the API is still the security boundary.
- **Why:** It covers user switcher, create, inbox, detail, audit, and error states within the assessment timebox.

## Three things AI got wrong

1. It initially treated the API prompt as the full assessment; the PDF also requires frontend, tests, documentation, and Git evidence.
2. It initially allowed any seeded user to retrieve request detail. The code was changed to enforce server-side read authorization.
3. It initially relied on an idempotency pre-check. The final design also uses the database unique constraint to survive a duplicate-submit race.
