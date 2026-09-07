# FactFlow

FactFlow is an ASP.NET Core MVC application that retrieves data from the [Cat Fact API](https://catfact.ninja/fact) and appends every successful response to a local text file.

The required assignment is implemented as a complete vertical slice with dependency injection, asynchronous I/O, explicit CQRS handlers, authentication, automated tests and a custom dashboard.

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
- Download the complete text journal.
- Add facts manually with server-side validation and calculated length.
- Inspect technical history: declared/calculated length, integrity, duplicate detection, SHA-256 and raw JSON.
- Use multiple persistent workspace tabs modelled after a desktop application.
- Preserve unsaved manual-entry drafts while switching tabs.
- Authenticate through an ASP.NET Core cookie.
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
    CatFact model          typed HTTP client + JSONL journal
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
```

The dashboard and history use query handlers. Fetching and manual creation use command handlers. The web layer does not implement HTTP or file persistence directly.

## Solution structure

| Project | Responsibility |
|---|---|
| `FactFlow.Domain` | Core Cat Fact model |
| `FactFlow.Application` | Use cases, CQRS contracts and handlers |
| `FactFlow.Infrastructure` | External API and filesystem implementations |
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

## Configuration

`FactFlow.Web/appsettings.json`:

```json
{
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
```

## Next iterations

- SQL Server persistence through EF Core.
- Full fact CRUD.
- Fact ratings and credibility votes.
- Azure App Service and Azure SQL deployment.
- Optional Microsoft Dataverse integration.
