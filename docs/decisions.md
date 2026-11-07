# Decisions

Choices made where CLAUDE.md was silent or where reality forced a change. Newest last within each phase.

## Phase 1

| ID | Decision | Why |
| --- | --- | --- |
| D-01 | `global.json` pins SDK `10.0.100` with `rollForward: latestFeature`. | Any 10.0.x SDK works on the shop PC and CI; a hard pin to one patch would break builds for no benefit. |
| D-02 | `Directory.Build.props` sets `EnableWindowsTargeting=true`. | Lets the WPF project restore and compile on macOS/Linux. The app still only **runs** on Windows. |
| D-03 | Analyzers: `AnalysisLevel=latest-recommended`, `TreatWarningsAsErrors=true`. `.editorconfig` switches off only a few noisy rules (CA2007, CA1303, CA1848 and similar), each with a reason. | Rule 10 (warnings as errors, analyzers on) without drowning in rules that do not apply to a desktop app. |
| D-04 | Ids are `long` auto-increment (`Entity.Id`). | Simple, small indexes, fine for a single-file SQLite database. |
| D-05 | Use cases return `Result` / `Result<T>` whose error is a **code** (`Login_Invalid`). The Desktop project maps the code to a `.resx` string. | Keeps all user text in `.resx` (rule 6) and out of Application/Domain. A test checks every code has an English and Urdu string. |
| D-06 | `IUseCaseRunner` opens a DI scope (and a fresh `DbContext`) for each use case call. ViewModels call services through it. | A desktop app has no request scope. One long-lived `DbContext` would go stale and is not thread-safe. |
| D-07 | `AppDbContext` implements `IUnitOfWork`. Repositories only stage changes; the use case calls `SaveChangesAsync` once. | One save = one transaction, which Phase 4 needs for "one sale = one transaction" (rule 3). |
| D-08 | Role access to screens (`RoleAccess`): Owner all; Manager all; Cashier Billing, Udhaar, Settings; Store keeper Stock count, Purchases, Settings. **Everyone** can open Settings. Cashier has no Dashboard. | Settings holds language and theme, which every user needs. Owner-only items (users, backup, prices) will be gated inside the page when they arrive. Manager "no user or settings control" is enforced at that level, not by hiding the page. Adjust the table in one place if the owner disagrees. |
| D-09 | Passwords: PBKDF2-HMAC-SHA256, 600,000 iterations, 16-byte salt, stored as `pbkdf2-sha256$iter$salt$hash`. Built in to .NET, so no package. | Rule 8. Iteration count is stored in the hash so it can be raised later. Unknown usernames are verified against a dummy hash so timing does not reveal valid names. |
| D-10 | First-run admin: username `admin`, password `Admin@123`, role Owner, **must change at first login**. | The spec requires a forced change. A known first password is the simplest workable start for a shop PC. The change screen blocks everything else until done. |
| D-11 | New password rule: at least 8 characters with a letter and a digit, different from the current one. | Simple enough for shop staff, still blocks the worst choices. |
| D-12 | SQLite connection string uses `Pooling=False`, `Foreign Keys=True`. WAL is switched on in `DatabaseInitializer`. | With pooling off the file handle is released when a connection closes, which Phase 6 backup and restore needs. |
| D-13 | Dates are stored as UTC and read back with `Kind=Utc` through a value converter. | SQLite has no date type. Tests check the round trip. |
| D-14 | Language is `en` or `ur` (culture `ur-PK`). Saved in the `Settings` table, applied at startup before the login screen. Theme is `light` or `dark`. | Spec: two languages from day one, light and dark theme. |
| D-15 | Urdu text uses the bundled **Noto Nastaliq Urdu** font (variable TTF from google/fonts, SIL OFL 1.1, licence file next to it). WPF renders the default weight of a variable font. Line height is raised to 40 for Urdu. | Spec section 7. The WPF-UI theme font resource `ContentControlThemeFontFamily` is overridden at runtime so WPF-UI controls also switch font. **Needs a visual check on Windows.** |
| D-16 | One window, one `RootViewModel`: Login, then forced password change if flagged, then Shell. Navigation is a left list (not WPF-UI `NavigationView`). | Keeps ViewModels free of UI types and the flow unit-testable. |
| D-17 | Screens not built yet show a "coming in a later phase" placeholder page. | The shell and role filtering can be tested now. |
| D-18 | Only one copy of the app may run (named mutex). | Two copies on one SQLite file invites confusion; the second shows a message and exits. |
| D-19 | One test project, `HassanTraders.Tests` (folders `Domain/`, `Application/`, `Infrastructure/`, `Desktop/`), compiles the UI-free production code (Core, Data, Services, ViewModels, localization) and the `.resx` files straight from source. | WPF cannot load on macOS/Linux, and the spec wants testable ViewModels. One project instead of four keeps the solution simple; the same tests run on Windows. XAML, code-behind and `Common/` stay out by design. |
| D-20 | Domain `Quantity` is **not** in Phase 1. | Phase 2 owns bag-plus-kilo maths. `Money` (including integer-only `Proportion` for weighed goods) is in. |

## Phase 2 (in progress)

| ID | Decision | Why |
| --- | --- | --- |
| D-21 | `Quantity` stores a signed 64-bit count of base units; weighed quantities use grams, while piece-based items use whole pieces. Bag notation treats one bag as 25,000 grams. | Keeps stock arithmetic integer-only and lets future stock movements express either grams or pieces without floating point. |
| D-22 | Item pack size and reorder level are stored as integer base units for their selling unit; item prices remain paisa. Tax rate is stored as integer basis points. | Avoids implicit decimal conversions and supports fixed-size packs and loose goods with one model. |
| D-23 | The sample CSV is the single seed/import fixture. Multiple rows with the same category, type, and item name plus a variant name create variants rather than duplicate products. | Tests and first-run catalogue seed use the same source data. |
| D-24 | ClosedXML 0.104.2 (MIT) reads XLSX imports; CSV support uses the .NET base libraries. | Adds spreadsheet import without requiring Office on the shop PC. |
| D-25 | All production code is compiled into `HassanTraders.Desktop` and organized into `Core`, `Services/Application`, `Data`, `Features`, and `Common`; the tests live in the single `HassanTraders.Tests` project and compile the relevant UI-free sources directly. | Keeps the solution simple for this single-user WPF application while preserving clear folders and cross-platform unit-test execution. |
| D-26 | **Design system** (UI phase): colour tokens in `Resources/Themes/Colors.{Light,Dark}.xaml`, brushes in `Styles/Brushes.xaml`, styles in `Resources/Styles/`, controls in `Common/Controls/`. Everything is a `DynamicResource`, so the theme switch swaps one dictionary and the whole UI follows. | One source of truth; changing the brand colour is one line per theme. Documented in `docs/design-system.md`. |
| D-27 | The light-theme status colours are darker than first proposed: Success `#17784A` (was `#1E8E5A`), Warning `#9A6414` (was `#B7791F`), Info `#34639F` (was `#3B6FB6`). | The first values gave 3.3 to 4.4 : 1 contrast on their tinted backgrounds, below the WCAG AA 4.5 : 1 needed for small pill text. A test now enforces 4.5 : 1 for every text/background pair in both themes. The brand primary `#0B6FA4` is unchanged. |
| D-28 | WPF-UI stays as the base (window, title bar, icons, scroll bars, tooltips). Buttons, text boxes, combo boxes, check boxes, data grids and progress bars are our own templates; the WPF-UI accent is re-set to the brand primary and the Windows accent colour is not used. | The spec fixes WPF-UI. Own templates for the visible controls guarantee one look that does not depend on WPF-UI internals; the rest follows the brand through the accent. |
| D-29 | The window has no Mica backdrop; the title bar is navy so it joins the sidebar. The title bar stays left-to-right in Urdu. | Mica tints the background by wallpaper, which breaks one consistent palette. Caption buttons stay where Windows users expect them. |
| D-30 | Body text is 16 pt in Urdu (14 pt in English), line height 40, and Urdu secondary lines on English screens keep the Nastaliq font with line height 26. | Nastaliq has tall ascenders and descenders and is clipped at normal line height; this keeps Urdu as readable as English. **Needs a visual check on Windows.** |
| D-31 | Items and Dashboard are real screens on real data via a read-only `ICatalogReader` and `ICatalogQueryService`. The dashboard shows catalogue counts only; sales, stock alerts and udhaar cards are honest empty states until phases 3 to 5. Items has no stock-status pill yet (there is no stock ledger), only Active / Inactive. | No fake numbers (UI instruction 5). |
| D-32 | Items are filtered in memory with a single grid reset (`BulkObservableCollection`), and search ignores case, diacritics and Arabic/Urdu letter variants (`SearchNormalizer`). A test checks 10,000 items filter in well under 0.5 s. | Acceptance criterion in section 10; Urdu keyboards differ in how they type yeh, kaf and heh. |
| D-33 | `NavigationService` creates pages with `ActivatorUtilities` and `LocalizedViewModel.Dispose` is idempotent. | The container otherwise keeps every disposable page alive for the whole session and disposes it a second time at shutdown. Found by a test when the first data page was added. |
| D-34 | Custom `DialogWindow` (info, error, confirm, destructive) replaces `MessageBox` for in-app messages. Start-up errors before the theme is loaded still use the native box. | Elegant, themed, right-to-left aware confirmations. Enter confirms, Esc cancels. |

## Package licences

Checked from each package's `.nuspec` on 2026-11-07.

| Package | Version | Licence |
| --- | --- | --- |
| CommunityToolkit.Mvvm | 8.4.2 | MIT |
| ClosedXML | 0.104.2 | MIT |
| FluentValidation, FluentValidation.DependencyInjectionExtensions | 12.2.0 | Apache-2.0 |
| Microsoft.EntityFrameworkCore.Sqlite / .Design | 10.0.14 | MIT |
| Microsoft.Extensions.Hosting / DependencyInjection(.Abstractions) / Configuration | 10.0.14 | MIT |
| Serilog | 4.4.0 | Apache-2.0 |
| Serilog.Extensions.Hosting | 10.0.0 | Apache-2.0 |
| Serilog.Settings.Configuration | 10.0.1 | Apache-2.0 |
| Serilog.Sinks.File | 7.0.0 | Apache-2.0 |
| WPF-UI | 4.3.0 | MIT |
| xunit / xunit.runner.visualstudio | 2.9.3 / 3.1.5 | Apache-2.0 |
| NSubstitute | 6.2.0 | BSD-3-Clause |
| Microsoft.NET.Test.Sdk | 18.11.0 | MIT |
| Microsoft.Extensions.Logging.Abstractions | 10.0.14 | MIT |
| Noto Nastaliq Urdu (font, bundled) | google/fonts | SIL OFL 1.1 |

Not added yet (check when added): QuestPDF (Phase 6; has its own community-licence terms that depend on company revenue, so read them before adding), Velopack (Phase 7).
