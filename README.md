# Access Request Hub

Local MVP for the AGIT Phase 1 assessment. It provides a .NET 8 API, PostgreSQL schema/migrations, a small browser UI, and seeded demo identities.

## Run

1. Create a PostgreSQL database and set `ConnectionStrings__DefaultConnection` (do not commit a real password). Example PowerShell: `$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=access_request_hub;Username=postgres;Password=your-password'`.
2. Run `dotnet run --project src/AccessRequestHub.Api`.
3. Open the displayed local URL. Swagger is at `/swagger`.

Startup applies migrations and seeds Alice, Bob, Carol, Dana, Erin, CRM, and Finance Portal. The UI's user switcher sends the required `x-user-email` header; authorization remains entirely server-side.

## Test

Run `dotnet test`. The automated tests cover standard/high-risk flow, authorization, idempotency, and a stale version conflict.

## Demo flow

Create as Alice. Bob approves a NonProduction/Read request to Approved. For Production or Admin, Bob's approval moves it to PendingSystemOwner; Carol approves CRM and Dana approves Finance Portal. Rejection needs a reason and the timeline records it.

## Security notes

`appsettings.json` intentionally has a non-working placeholder connection string. Supply credentials through environment variables or local secret storage. This MVP simulates authentication only; a real deployment requires an authenticated identity provider.
