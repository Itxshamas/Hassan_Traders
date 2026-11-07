# Architecture

The solution has one production project, `HassanTraders.Desktop`. Source folders preserve clear Core, Application-service, Data, and WPF responsibilities without separate production assemblies or project references.

```text
HassanTraders.Desktop
├── Core/                 Entities, value types, interfaces and DTOs
├── Services/Application/ Use cases, validators and application registration
├── Data/                 EF Core, repositories, migrations, import and seed
├── Features/             Feature views and view models
├── Common/               WPF converters, helpers and controls
├── Resources/            Localization, styles, themes and templates
└── App.xaml.cs           Host, logging and DI composition root
```

There is one test project, `HassanTraders.Tests`, with folders `Domain/`, `Application/`, `Infrastructure/` and `Desktop/`. It compiles the UI-free production source (Core, Data, Services, ViewModels, `.resx`) into the test assembly, because WPF cannot be loaded on macOS/Linux.

## Phase 1 runtime flow

1. `App.OnStartup` installs the global exception handlers, takes the single-instance mutex, builds the host (Serilog log in `%LocalAppData%\HassanTraders\logs`).
2. `IDatabaseInitializer` creates the database, applies migrations, switches on WAL, seeds the admin user.
3. `IAppearanceService` loads the saved language and theme and applies them.
4. `RootViewModel` shows `LoginViewModel`. On success: `ChangePasswordViewModel` if the account is flagged, then `ShellViewModel`.
5. `ShellViewModel` lists only the screens `RoleAccess` allows for the user's role. `NavigationService` creates the page ViewModel.

## Rules in practice

- **No business rules in views.** Code-behind only calls `InitializeComponent()` (and focuses the username box).
- **User text** lives in `Resources/Strings.resx` and `Strings.ur.resx`. XAML uses `{wpf:T Key}`, ViewModels use `ILocalizationService`. Use cases return error *codes*.
- **Language switch** flips `FlowDirection`, font and line height on the root grid, and overrides the WPF-UI font resource.
- **Persistence**: repositories stage, the use case saves once through `IUnitOfWork`. `IUseCaseRunner` gives each call its own scope and `DbContext`.
- **Testability**: ViewModels hold no WPF types. `HassanTraders.Tests` compiles them from source so tests run on any OS.

## Catalogue (Phase 2 in progress)

- `Category`, `ProductType`, `Item`, `Variant`, `UnitKind`, and integer `Quantity` live under `Core`.
- `ItemCatalogImportService` validates import rows and stages entities through `IItemRepository`; `CatalogFileReader` reads CSV and XLSX using ClosedXML.
- `tests/TestData/items.csv` is also embedded in the main project and seeds 38 items (including four Lays variants) on first database initialization.
- Prices are stored as whole paisa. Pack and reorder quantities are integer base units; weighed-stock quantities use grams. A bag converts to 25 kilograms.
- The catalogue management screens and owner-facing category/type maintenance are not implemented yet.

## UI design system

Colours, type, spacing, components and page patterns are defined once in `Resources/` and `Common/Controls/` and documented in [design-system.md](design-system.md). Every screen must use them; `DesignSystemTests` enforces the mechanical rules.
