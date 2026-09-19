# Production-readiness self-review

| Finding | Severity | Action | Status | Evidence |
|---|---|---|---|---|
| Credential leakage risk in local configuration | High | Tracked configuration uses placeholders; README documents environment-variable setup | Fixed | `appsettings.json`, README, repository scan |
| Duplicate create can race | High | Unique `ClientRequestId` index plus PostgreSQL unique-violation recovery | Fixed | context, migration, `RequestService` |
| Two approvers can act on stale data | High | PostgreSQL `xmin` token; API returns HTTP 409 for a stale version | Fixed | entity mapping, `RequestService`, stale-version test |
| State update and audit event could diverge | High | Request state and audit event are saved in one database transaction | Fixed | `RequestService` |
| Header authentication is forgeable | Medium | Deliberately limited to the local MVP; production requires authenticated identity | Deferred | README |
| Tests do not run against PostgreSQL | Medium | Documented limitation; PostgreSQL integration testing is needed to prove physical `xmin` behavior and a real concurrent race | Deferred | PLAN.md, tests |
| High-risk/rejection audit test coverage is incomplete | Medium | Manual demo flow is documented; add integration tests before production use | Deferred | README, `RequestServiceTests.cs` |
| Migration history was replaced during development | Medium | Use a fresh local database for the demo; an upgrade path for a prior database is not supplied | Deferred | README, `Data/Migrations` |
| `assessment-start` chronology cannot be proven from the available history | Medium | Tag points to the earliest available commit. It was added after the historical start point and is recorded as a known evidence limitation. | Known limitation | `git show assessment-start` |
| `phase-1-complete` tag | Info | Tag points to the final Phase 1 commit after build and automated tests passed | Fixed | `git show phase-1-complete` |

## Verification evidence

Verified on 2026-09-19:

- `dotnet build AccessRequestHub.slnx --no-restore`: passed, 0 warnings, 0 errors.
- `dotnet test AccessRequestHub.slnx --no-restore`: passed, 4 tests, 0 failures.
- Tracked-file scan found only documented database-password placeholders, not a real credential.
- `git show assessment-start` and `git show phase-1-complete` confirm both required tag names exist.

## Known limitations and deferred work

- No SSO/OIDC, notification integration, rate limiting, or production observability stack; these are outside the assessment scope.
- The frontend intentionally has no advanced search, pagination, or analytics.
- PostgreSQL is required locally to run migrations and the browser demo.
- The earliest repository commit already contains a substantial project state. The `assessment-start` tag therefore identifies the earliest available commit, rather than independently proving a pre-implementation baseline.
