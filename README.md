# FactFlow

FactFlow is an ASP.NET Core MVC application that retrieves data from the [Cat Fact API](https://catfact.ninja/fact) and appends every successful response to a local text file.

The required assignment is implemented as a complete vertical slice with dependency injection, asynchronous I/O, explicit CQRS handlers, SQL Server persistence, authentication, automated tests and a custom dashboard.

## Assignment coverage

| Requirement | Implementation |
|---|---|
| Microsoft technology | .NET 10, C#, ASP.NET Core MVC and Razor |
| Connect to the endpoint | Typed `HttpClient` calls `https://catfact.ninja/fact` asynchronously |
| Create a local `.txt` file | Created automatically after the first successful response |
| Append every response on a new line | Compact UTF-8 JSON Lines format; existing content is never overwritten |
| Dependency Injection | Built-in Microsoft DI with interface-based infrastructure services |
| Version control | Repository-ready solution with generated/runtime data excluded |

Example output:

```json
{"fact":"Cats sleep for much of the day.","length":32}
{"fact":"A second response.","length":18}
```

## Features

- Fetch and save a random Cat Fact.
- View the latest response and journal statistics.
- Download the synchronized text file.
- Add facts manually with server-side validation and calculated length.
- Inspect technical history: declared/calculated length, integrity, duplicate detection, SHA-256 and raw JSON.
- Use multiple persistent workspace tabs modelled after a desktop application.
- Preserve unsaved manual-entry drafts while switching tabs.
- Authenticate through an ASP.NET Core cookie.
- Manage SQL-backed facts through create, read, update and soft-delete operations.
- Keep the required TXT file synchronized with active SQL records.
- Bootstrap an empty database once from an existing TXT file, then use SQL as the source of truth.
- Check application availability at `/health`.

## Quick start

### Visual Studio

1. Open `FactFlow.slnx`.
2. Set `FactFlow.Web` as the startup project.
3. Select the `https` launch profile.
4. Press `F5`.
5. Open `https://localhost:7046` if the browser does not launch automatically.

### .NET CLI

```powershell
dotnet restore
dotnet run --project .\FactFlow.Web\FactFlow.Web.csproj --launch-profile https
```

Development login:

```text
Username: operator
Password: FactFlow2026!
```

These credentials are limited to `appsettings.Development.json`. Deployed environments must provide their own values through configuration or environment variables.

The development profile uses SQL Server LocalDB. Visual Studio normally installs it automatically. On first startup, EF Core creates `FactFlowDb`, applies migrations and imports existing TXT lines.

## Verify the core requirement

1. Log in.
2. Click **Download and save**.
3. Confirm the response appears under **Latest response**.
4. Confirm the saved-response counter increases.
5. Open `FactFlow.Web/App_Data/catfacts.txt` or click **Download TXT**.
6. Repeat the request and verify that a new JSON object was appended on a new line.

`App_Data/catfacts.txt` is runtime data and is intentionally excluded from Git.

## Architecture

```text
FactFlow.Web
    MVC controllers + Razor views + authentication + workspace UI
            |
            v
FactFlow.Application
    Commands / queries + handlers + infrastructure abstractions
          /   \
         v     v
FactFlow.Domain        FactFlow.Infrastructure
    fact entities          HTTP + JSONL + EF Core SQL Server
```

Request flow:

```text
POST /Facts/Fetch
    -> FetchCatFactCommandHandler
    -> ICatFactClient
    -> https://catfact.ninja/fact
    -> response validation
    -> IFactJournal.AppendAsync
    -> App_Data/catfacts.txt
    -> IFactRepository.AddAsync
    -> SQL Server Facts table
```

The dashboard and CRM list read from SQL Server. Technical history reads the synchronized TXT projection. Fetching, manual creation, editing and soft deletion use command handlers. The web layer does not implement HTTP, file or database persistence directly.

SQL Server is the source of truth. Creates commit to SQL and append one TXT line. Updates and soft deletes commit to SQL and regenerate the TXT projection from active records. On first startup only, an empty database can be bootstrapped from an existing assignment file; afterward startup synchronization always flows from SQL to TXT.

## Solution structure

| Project | Responsibility |
|---|---|
| `FactFlow.Domain` | Cat Fact and mutable CRM record models |
| `FactFlow.Application` | Use cases, CQRS contracts, CRUD and handlers |
| `FactFlow.Infrastructure` | External API, filesystem, EF Core and SQL Server |
| `FactFlow.Web` | MVC controllers, Razor UI, authentication and session workspaces |
| `FactFlow.Tests` | Application and infrastructure tests |

## Tests

Run all tests:

```powershell
dotnet test .\FactFlow.slnx
```

Current suite covers:

- API response deserialization and HTTP failures.
- Exact JSON line format and append behavior.
- Malformed journal-line handling.
- Fetch-command validation and persistence.
- Manual-fact normalization and calculated length.
- Dashboard statistics.
- History integrity, duplicate detection, hashing and raw JSON.
- Database create, update and soft-delete behavior.

## Local database

Development connection:

```text
Server=(localdb)\FactFlowLocalDb;Database=FactFlowAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True
```

Inspect it in Visual Studio:

1. Open **View → SQL Server Object Explorer**.
2. Expand **SQL Server → (localdb)\FactFlowLocalDb → Databases**.
3. Expand **FactFlowAppDb → Tables → dbo.Facts**.
4. Right-click `dbo.Facts` and select **View Data**.

Apply migrations manually if required:

```powershell
dotnet tool restore
dotnet ef database update `
  --project .\FactFlow.Infrastructure\FactFlow.Infrastructure.csproj `
  --startup-project .\FactFlow.Web\FactFlow.Web.csproj
```

## Production configuration boundary

`appsettings.Production.json` intentionally contains no secrets and disables startup migrations. Configure these values in Azure App Service **Environment variables**:

```text
ConnectionStrings__FactFlow          Azure SQL connection string
DemoAuth__Username                   temporary private-demo username
DemoAuth__Password                   temporary private-demo password
FactJournal__Path                    {HOME}/data/catfacts.txt
DataProtection__Path                 {HOME}/data/keys
Database__ApplyMigrationsOnStartup  false
```

The Azure SQL connection should be supplied as an App Service connection string named `FactFlow`; `GetConnectionString("FactFlow")` reads it without changing code. Apply EF migrations separately before opening the deployed app. Replace demo authentication with Microsoft Entra ID before public exposure.

## Configuration

`FactFlow.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "FactFlow": "configured per environment"
  },
  "Database": {
    "ApplyMigrationsOnStartup": false
  },
  "CatFactApi": {
    "Endpoint": "https://catfact.ninja/fact"
  },
  "FactJournal": {
    "Path": "App_Data/catfacts.txt"
  }
}
```

Environment variables use standard ASP.NET Core double-underscore notation:

```text
CatFactApi__Endpoint
FactJournal__Path
DemoAuth__Username
DemoAuth__Password
ConnectionStrings__FactFlow
Database__ApplyMigrationsOnStartup
```

## Next iterations

- Fact ratings and credibility votes.
- Azure App Service and Azure SQL deployment.
- Optional Microsoft Dataverse integration.
