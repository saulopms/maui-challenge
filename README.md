# Maui DEX Challenge

This repository contains a hiring challenge solution built with:

- `.NET MAUI` for the client application
- `ASP.NET Core Minimal API` for ingestion
- `SQL Server / LocalDB` scripts and stored procedures for persistence
- shared parsing/domain code to keep the solution cohesive and testable

## Solution structure

```text
src/
  MauiDexChallenge.App      -> .NET MAUI client using MVVM
  MauiDexChallenge.Api      -> ASP.NET Core Minimal API
  MauiDexChallenge.Shared   -> shared domain models, parser, contracts
sql/
  001-create-database.sql   -> creates LocalDB database if needed
  002-schema.sql            -> creates tables and stored procedures
docs/
  ai-assisted-development.md
```

## Architecture notes

### MAUI App

- Uses `MVVM` with a dedicated `MainViewModel`
- Uses an `IDexSubmissionService` abstraction for HTTP communication
- Keeps the two required DEX payloads hardcoded in `DexReportCatalog`
- Allows editing the API base URL so the app can target localhost, emulator loopback, or a LAN address

### Shared layer

- Contains `DEX` parsing logic in `DexParser`
- Extracts only the fields requested by the challenge:
  - `DEXMeter`
  - `DEXLaneMeter`
- Avoids duplicating parsing rules between app and API

### API

- Exposes `POST /vdi-dex?machine=A|B`
- Accepts the DEX report in the raw HTTP body
- Enforces HTTP Basic Authentication using:
  - username: `vendsys`
  - password: `NFsZGmHAGWJSZ#RuvdiV`
- Parses the DEX payload in C#
- Persists data through two stored procedures:
  - `dbo.SaveDexMeter`
  - `dbo.SaveDexLaneMeter`

## Database design

### Tables

- `dbo.DEXMeter`
  - machine
  - DEX date/time
  - machine serial number
  - value of paid vends
  - raw DEX content
- `dbo.DEXLaneMeter`
  - foreign key to `DEXMeter`
  - product identifier
  - price
  - number of vends
  - value of paid sales

### Constraints

- Unique constraint on `(Machine, DexDateTime)` to avoid duplicate DEX meter inserts for the same machine snapshot
- Foreign key from `DEXLaneMeter` to `DEXMeter`

## How to run

### 1. Restore

```powershell
dotnet restore MauiDexChallenge.slnx
```

### 2. Start the API

```powershell
dotnet run --project .\src\MauiDexChallenge.Api\MauiDexChallenge.Api.csproj
```

Default local endpoint:

```text
http://localhost:5225
```

The API is configured to initialize the database schema on startup through the SQL scripts copied to the output folder.

### 3. Start the MAUI app on Windows

```powershell
dotnet build .\src\MauiDexChallenge.App\MauiDexChallenge.App.csproj -f net10.0-windows10.0.19041.0
dotnet run --project .\src\MauiDexChallenge.App\MauiDexChallenge.App.csproj -f net10.0-windows10.0.19041.0
```

### 4. Use the UI

- Keep the API base URL as `http://localhost:5225` on Windows
- Use `10.0.2.2` or a LAN IP if testing from an Android emulator/device
- Click `Send Machine A` or `Send Machine B`

## Build validation performed

These builds completed successfully in the current environment:

```powershell
dotnet build .\src\MauiDexChallenge.Api\MauiDexChallenge.Api.csproj
dotnet build .\src\MauiDexChallenge.App\MauiDexChallenge.App.csproj -f net10.0-windows10.0.19041.0
```

## Notes about LocalDB backup

The challenge requests a `.bak` file. In this environment:

- the solution includes the complete database creation scripts
- LocalDB was detected
- automated `sqlcmd` execution hit a local client encryption/connection limitation

Because of that, the repository includes the schema and stored procedures, but the `.bak` generation should be executed on the target machine after the LocalDB instance is confirmed available.

Suggested manual backup command:

```sql
BACKUP DATABASE [MauiDexChallengeDb]
TO DISK = 'C:\path\to\MauiDexChallengeDb.bak'
WITH INIT, FORMAT;
```

## Interview-ready talking points

- Why the parser lives in a shared project
- Why the app depends on an interface instead of calling `HttpClient` directly in the ViewModel
- Why duplicate DEX snapshots are blocked at the database layer
- Why the API keeps authentication simple but isolated behind a dedicated validator
- How the structure supports future testing and feature growth
