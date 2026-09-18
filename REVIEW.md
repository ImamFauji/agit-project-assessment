# Self-review

| Finding | Severity | Action | Status | Evidence |
|---|---|---|---|---|
| Committed database password | High | Replaced with a placeholder; document environment configuration | Fixed | `appsettings.json`, README |
| Duplicate create race | High | Unique index plus `23505` recovery | Fixed | migration and `RequestService` |
| Approval race | High | PostgreSQL `xmin` concurrency token and conflict response | Fixed | entity mapping/service |
| Real authentication absent | Medium | Deliberately simulated only for MVP | Deferred | README |
| No production telemetry | Low | Standard application logging remains enabled | Deferred | `appsettings.json` |

Known limitation: automated integration tests require a PostgreSQL test database to exercise the provider's physical `xmin` update behavior; the included tests validate service-level stale-version handling.
