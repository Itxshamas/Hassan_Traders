# Hassan Traders

Billing, stock and udhaar (credit) app for Hassan Traders. Windows desktop (WPF, .NET 10), English and Urdu (right-to-left). Requirements: [CLAUDE.md](CLAUDE.md). Decisions: [docs/decisions.md](docs/decisions.md). UI rules: [docs/design-system.md](docs/design-system.md).

## Requirements

- Windows 10/11 to run the app. Visual Studio 2022 (17.14 or newer) or VS Code with C# Dev Kit.
- .NET 10 SDK.
- macOS/Linux can build and run the non-UI tests, but not launch the app.

Check the selected SDK with `dotnet --version` (the repository pins .NET 10 and allows compatible feature-band SDKs). The catalogue import supports CSV and XLSX; the shared sample file is [tests/TestData/items.csv](tests/TestData/items.csv).

## Build, test, run

```powershell
dotnet restore
dotnet build HassanTraders.sln
dotnet test HassanTraders.sln
dotnet run --project src/HassanTraders.Desktop
```

## First sign-in

Username `admin`, password `Admin@123`. You must choose a new password straight away.

Data and logs: `%LocalAppData%\HassanTraders\` (`hassantraders.db`, `logs\app-YYYYMMDD.log`). To use another database file while testing, set `Database:Path` in `appsettings.json` next to the exe.

## Database migrations

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --project src/HassanTraders.Desktop --startup-project src/HassanTraders.Desktop --output-dir Data/Migrations
```

The app applies pending migrations at startup. A test fails if the model changed without a migration.

## Release

Installer and update channel (Velopack) arrive in Phase 7.

## Layout

See [docs/architecture.md](docs/architecture.md).
