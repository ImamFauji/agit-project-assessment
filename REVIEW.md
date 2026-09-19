# Production-readiness self-review

| Finding | Severity | Action | Status | Evidence |
|---|---|---|---|---|
| A real database password was present in tracked configuration | High | Replaced with `CHANGE_ME`; README requires an environment variable/local secret | Fixed | `appsettings.json`, README, repository scan |
| Duplicate create can race | High | Unique `ClientRequestId` index plus PostgreSQL unique-violation recovery | Fixed | context, migration, `RequestService` |
| Two approvers can act on stale data | High | PostgreSQL `xmin` token; API returns HTTP 409 for stale version | Fixed | entity mapping, service, test |
| State update and audit event could diverge | High | Save both changes in one database transaction | Fixed | `RequestService` |
| Header authentication is forgeable | Medium | Deliberately limited to local MVP; production needs real authentication | Deferred | README |
| Tests do not use PostgreSQL | Medium | Documented limitation; `xmin` needs PostgreSQL integration testing | Deferred | PLAN.md, tests |
| Required Git tags are absent | High | Candidate must create accurate tags after reviewing history and test result | Open | `git tag --list` audit |

## Verification evidence

- `dotnet test AccessRequestHub.slnx --no-restore`: passed, 4 tests, 0 failures.
- `dotnet build AccessRequestHub.slnx --no-restore`: passed.
- Tracked-file scan found no real credential after the configuration fix.

## Known limitations and deferred work

- No SSO/OIDC, notifications, rate limiting, or production observability stack; outside assessment scope.
- The frontend intentionally has no advanced search, pagination, or analytics.
- PostgreSQL is required locally to run migrations and the browser demo.
