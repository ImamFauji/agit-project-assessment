# AI Usage

1. Ask: compare the existing project with the assessment requirements. Result: identified missing frontend, tests, evidence docs, detail authorization, and race-safe duplicate handling. Accepted after code review.
2. Ask: improve service-layer workflow safety. Result: transaction-scoped audit writes, role checks, and unique-constraint recovery. Accepted; PostgreSQL-specific recovery is intentional because this project uses Npgsql.
3. Ask: create a minimal local workflow UI. Result: vanilla HTML/JS to avoid unnecessary client build complexity. Accepted after reviewing that every sensitive action is still server-authorized.

## Three things AI got wrong

1. It initially treated the API prompt as the full assessment; the PDF adds mandatory frontend, test, documentation, and Git evidence.
2. It initially allowed any seeded user to read a request detail; this is now restricted to requester, relevant manager/owner, or auditor.
3. A pre-check alone is not safe idempotency. The unique database index and duplicate-key recovery are required for concurrent submissions.
