# Access Request Hub

Local MVP for the AGIT Phase 1 assessment. It provides a .NET 8 Razor MVC interface, required API endpoints, PostgreSQL schema/migrations, and seeded demo identities.

## Setup

Prerequisites:

- .NET 8 SDK
- PostgreSQL running on the configured host and port
- An existing PostgreSQL database, for example `agit-db`

From the repository root, configure the connection string through an environment variable. Do not commit a real password to the repository.

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=agit-db;Username=YOUR_DATABASE_USERNAME;Password=YOUR_DATABASE_PASSWORD'
```

Git Bash:

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=agit-db;Username=YOUR_DATABASE_USERNAME;Password=YOUR_DATABASE_PASSWORD'
```

Restore dependencies if needed:

```bash
dotnet restore AccessRequestHub.slnx
```

## Run

Start the MVC application from the repository root:

```bash
dotnet run --project src/AccessRequestHub.Api --launch-profile http
```

Open the URL shown by the console, normally `http://localhost:5000/`. The MVC interface is at `/`, and Swagger is at `/swagger`.

Stop the application with `Ctrl+C`.

## Migrate and Seed

The project contains EF Core migrations under `src/AccessRequestHub.Api/Data/Migrations`. Apply them explicitly with:

```bash
dotnet ef database update --project src/AccessRequestHub.Api
```

If the EF CLI is not installed:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

When the application starts, `DbSeeder` also calls `Database.MigrateAsync()` and inserts the demo applications and users when the application table is empty. Do not run the application against a database containing an unrelated schema. For a disposable local database, reset it with:

```bash
dotnet ef database drop --project src/AccessRequestHub.Api --force
dotnet ef database update --project src/AccessRequestHub.Api
```

## Test

Run all automated tests from the repository root:

```bash
dotnet test AccessRequestHub.slnx
```

The tests cover the standard and high-risk approval flows, authorization rules, idempotent request creation, and stale concurrency-version conflicts.

## Demo Users

The UI user switcher simulates the `x-user-email` request header. Authorization is still enforced by the server; selecting a user does not bypass the API rules.

| Email | Role / use |
|---|---|
| `alice@example.local` | Requester; reports to Bob and creates requests |
| `bob@example.local` | Manager; approves Alice's pending-manager requests |
| `carol@example.local` | CRM System Owner; approves CRM high-risk requests |
| `dana@example.local` | Finance Portal System Owner; approves Finance Portal high-risk requests |
| `erin@example.local` | Auditor; read-only access to all requests |

## Demo Flow

1. Select Alice and create a `NonProduction` + `Read` request. The request starts as `PendingManager`.
2. Select Bob and approve it. The request becomes `Approved`.
3. Select Alice and create a `Production` or `Admin` request. It requires both approval stages.
4. Select Bob and approve it. The request moves to `PendingSystemOwner`.
5. Select the matching owner: Carol for CRM or Dana for Finance Portal. Approve the request to move it to `Approved`.
6. To test rejection, select the current approver, open the request, enter a rejection reason, and choose `Reject`.
7. Open the request detail to review its audit timeline. Select Erin to verify read-only access to all requests.

## Security notes

`appsettings.json` intentionally has a non-working placeholder connection string. Supply credentials through environment variables or local secret storage. This MVP simulates authentication only; a real deployment requires an authenticated identity provider.
