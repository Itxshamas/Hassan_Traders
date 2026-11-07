# Hassan Traders: Desktop Billing and Stock App (WPF, .NET 10)

> Build on Windows 10/11 with the .NET 10 SDK and Visual Studio 2022 or VS Code. WPF cannot run on Linux or macOS.
> (On macOS/Linux the solution still restores and compiles, and the non-UI tests run, because `Directory.Build.props` sets `EnableWindowsTargeting`.)

## 1. Role and goal

You are a senior .NET engineer. Build a production-quality Windows desktop app for **Hassan Traders**, a Pakistani general store that also sells rice, dal and atta loose by the kilo. The shop today keeps a handwritten stock register in Urdu and English. The app replaces it: item catalogue, billing (POS), stock, purchases, customer credit (udhaar) ledger, receipts and reports.

Work in small, tested steps. After each phase, build, run the tests, and summarise what works and what is left. If something is unclear, choose the simplest sensible option, write the choice in `docs/decisions.md`, and continue. Do not stop to ask unless a choice cannot be undone.

## 2. Users

| Role | Can do |
| --- | --- |
| Owner | Everything: reports, prices, cost, users, backups, delete cleared records |
| Manager | Purchases, stock count, returns, discounts; no user or settings control |
| Cashier | Billing and receiving udhaar payments only |
| Store keeper | Receive goods, stock count, damage entry only |

## 3. Tech stack (fixed)

- .NET 10 (LTS), C#, WPF, MVVM with `CommunityToolkit.Mvvm`.
- `Microsoft.Extensions.Hosting` + DependencyInjection + Configuration (`appsettings.json`).
- SQLite through EF Core, WAL mode, migrations. One database file in `%LocalAppData%\HassanTraders\`.
- `FluentValidation`, `Serilog` (rolling file log), `xUnit` + `NSubstitute` for tests.
- UI kit: `WPF-UI` (Fluent look). Light and dark theme.
- Reports: `QuestPDF` (PDF) and `ClosedXML` (Excel). No Office needed on the shop PC.
- Installer and updates: `Velopack`.
- Package versions live in `Directory.Packages.props` (central package management). Pin every version. Check each package's licence and say so in `docs/decisions.md`.

## 4. Architecture rules

One production project, `HassanTraders.Desktop`, with code organized by responsibility. Core entities and application services remain separate by folder and namespace; data access and implementations live under `Data`. There is one test project, `HassanTraders.Tests`, separate from the WPF app.

```text
HassanTraders/
├── HassanTraders.sln
├── global.json  Directory.Build.props  Directory.Packages.props  .editorconfig  .gitignore  README.md
├── docs/            (SRS.md, architecture.md, decisions.md)
├── installer/  build/
├── src/
│   └── HassanTraders.Desktop/          single WPF production project
│       ├── Core/ (Entities, Interfaces, DTOs, Enums)
│       ├── Data/ (Context, Configurations, Repositories, Migrations, Import, Seed)
│       ├── Services/ (Application use cases, navigation, localization, appearance)
│       ├── Common/ (Helpers, Converters, Controls)
│       ├── Features/ (feature Views and ViewModels)
│       ├── Resources/ (Strings.resx, Strings.ur.resx, Styles, Themes, Templates)
│       ├── Assets/ (logo, Noto Nastaliq Urdu font)
│       └── App.xaml, MainWindow.xaml, appsettings.json
└── tests/
    ├── HassanTraders.Tests/            the single test project (folders Domain/, Application/, Infrastructure/, Desktop/)
    └── TestData/
```

Non-negotiable rules:

1. **Money and weight are whole numbers.** Store rupees as paisa (`long`) and weight in grams (`long`). No `double` or `decimal` for money or stock. Wrap them in small value types (`Money`, `Quantity`).
2. **Stock is a ledger.** Never edit a stock number directly. Every sale, purchase, return, damage and count writes a `StockMovement`. Stock on hand is the sum of movements per item.
3. **One sale = one database transaction.** Bill, lines, payments, stock movements and the customer balance are saved together or not at all.
4. **No business rules in Views or code-behind.** Views bind to ViewModels; ViewModels call application services; services hold the rules.
5. **One receipt model.** The 80 mm receipt, A5 invoice and payment voucher are all rendered from the same data, so a reprint always matches.
6. **Two languages from day one.** All user text is in `.resx`. A language switch flips `FlowDirection` to right-to-left for Urdu and swaps the font. Never hard-code visible text.
7. **Audit log** for price changes, deletions, discounts above a limit, stock adjustments and logins.
8. **Passwords** are hashed with a slow hash (PBKDF2 or BCrypt). Never store plain text.
9. Async I/O, no blocking the UI thread, global exception handler that logs and shows a friendly message.
10. Nullable enabled, warnings as errors, analyzers on.
11. **One design system.** Every screen uses the centralised design system in `Resources/` and `Common/Controls/` (section 12). No raw colours, no ad-hoc fonts or sizes, no per-module styling. `docs/design-system.md` is the single source of truth.

## 5. Features (build all; IDs are for tracking)

**Catalogue**
- F1 Item with Urdu name and English name, category, optional type, barcode, unit, pack size, cost price, sale price, reorder level, tax rate, active flag.
- F2 Variants of one product with different pack sizes and prices (for example Lays at Rs 20, 50, 70, 100).
- F3 Units: piece, dozen, carton, KG, gram, mL, litre, bag, with conversion (1 bag = 25 KG).
- F4 Loose items (rice, dal, sugar) are stored in grams. Show quantity as **bags plus loose kilos** (for example "5 bags 22 kg", the shop's "5:22" format) and the total in KG.
- F5 Search by Urdu name, English name, brand or barcode. Urdu keyboard input must work.
- F24 **Categories and types.** Category then optional Type. Owner can add both at any time. Seed: Snacks, Spices, Tea and pantry, Personal care, Cleaning, Rice (types: Premium, Superior, Middle order, Economical), Atta and flour, Dal and sugar. Billing, items and stock screens filter by category then type. Reports group by both.

**Stock**
- F6 Live stock, opening balance, low-stock alert at reorder level.
- F7 Stock count screen: type the counted quantity, see the difference from book stock, apply to write adjustment movements.
- F8 Expiry date per batch, expiry alerts, damage and return entries.

**Sales**
- F9 POS billing screen: barcode scan or typed name + Enter, tap an item tile, edit quantity, discount, hold and resume bills. Weighed items accept kilos with decimals.
- F10 Payment: cash, card, bank transfer, udhaar (credit), split payment.
- F11 Sale return with stock going back in.
- F21 **Printing.** Receipt opens after the sale (setting can turn off). Print, Reprint from the bills list (marked "COPY"), 80 mm thermal by default and A5 as an option. Receipt shows shop name, bill number, date, time, cashier, customer (credit sales), each line in Urdu and English with quantity, rate and amount, subtotal, discount, tax, total, payment method. Credit bills also show previous balance, this bill and new balance. Thermal printers lack an Urdu Nastaliq font, so **render the receipt as a WPF visual, convert it to a bitmap, and print the bitmap**. If the printer is offline the bill is still saved and can be reprinted. Reserve space for an invoice number and QR code (future FBR).

**Purchases and parties**
- F12 Purchase invoice from a supplier, cost update, supplier payment, supplier ledger.
- F13 Customer and supplier records with running balance and statement.
- F22 **Udhaar closes on full payment.** The open ledger lists only customers with balance above zero. A part payment lowers the balance and prints a payment voucher. A payment equal to the balance (or "Pay in full") closes the account: it leaves the open ledger and moves to "Cleared accounts" with the cleared date, and the voucher shows "ACCOUNT SETTLED". A payment above the balance is not accepted. If the customer takes credit again, the account reopens with its old statement. Permanent deletion of a cleared record is owner-only and audited. Keep cleared history by default.
- F23 **New customer while billing.** When the payment is udhaar, the cashier can pick "+ New customer" and type one name (Urdu or English); the bill is credited to the new account in the same step. The Udhaar screen also has a form with phone and opening balance (to copy balances from the paper register). Warn on a duplicate name or phone.

**Reports and data**
- F15 Daily sales, profit by item, stock value, low stock, expiry, customer dues, supplier dues, sales by category and type. Export to PDF and Excel.
- F16 Login with roles and the audit log.
- F17 Automatic daily backup of the database file to a second path (default 30 days kept) and a restore screen.
- F18 Import the item list from Excel or CSV with a report of rows that could not be read.
- F19 (later) Sales tax per item and an `IFiscalService` with a no-op implementation now; a real FBR POS connector can be added later without touching sales code.
- F20 (later) Cashier shift open and close with cash count.

## 6. Data model (tables)

Category, ProductType, Item, Variant (or pack size on Item), UnitConversion, StockBatch, StockMovement, Sale, SaleLine, SalePayment, SaleReturn, Purchase, PurchaseLine, Party (customer or supplier: phone, opening balance, status Open/Cleared, cleared date), LedgerEntry (every credit and payment; this is the statement), Payment voucher record, StockAdjustment, User, AuditLog, Setting. Sale has `PrintCount`. Add indexes on barcode, names and dates. All money columns are `long` paisa; all stock columns are `long` grams or pieces.

## 7. UI and UX

- Main window with a left navigation: Dashboard, Billing, Items, Stock count, Purchases, Udhaar ledger, Customers, Reports, Settings.
- Billing is keyboard-first: F2 focus search, Enter adds the first match, F9 completes the sale, F5 print, Esc cancels a dialog.
- Dashboard: today's sales, bill count, low or out of stock list, udhaar due, recent bills with Reprint, rice stock by type.
- Show status as pills (OK, Low, Out), not only numbers. Right-align numbers, use tabular digits.
- Urdu screens use a Nastaliq font (Noto Nastaliq Urdu, bundled), right-to-left, with enough line height. Both languages show the other language's name as a small second line on item rows.
- Large touch-friendly buttons on the billing screen; works at 1366x768.
- Look and feel, components and page patterns are defined once in section 12 and `docs/design-system.md`.

## 8. Seed data (from the shop's register)

Seed categories and types as in F24, an admin user (force a password change on first login), and a sample item list. Examples to include (Urdu and English names; prices are placeholders the owner edits):

- Snacks: Lays Classic at Rs 20, 50, 70, 100; Cheetos Rs 20.
- Spices: Shan Biryani Masala, National Tikka Masala, Swaad Achari Masala.
- Tea and pantry: Lipton Tea 430 g, Habibi Vinegar 500 mL, Soya Sauce 350 mL, Desi Ghee 900 g, Apple Jam 440 g.
- Personal care: Lifebuoy Soap, Sunsilk Shampoo 185 mL, Bio Amla Shampoo 100 mL, Vaseline 100 mL.
- Cleaning: Hero Surf 1 kg, Surf Excel, Toilet Cleaner 500 mL.
- Rice (loose, 25 kg bag): Premium: Inam Supreme, Inam Premium, Inam Gold. Superior: Inam Kainaat, Inam Mughal, 1121 Sella. Middle order: Inam Classic, PK 386 Sella, Irri-6. Economical: Irri-9, 1509 Sella.
- Atta: Inam Multigrain Atta 10 kg. Dal and sugar: Daal Mash (loose), Sugar (loose, 50 kg bag).

Put about 40 such items in `tests/TestData/items.csv` and use the same file to test the Excel/CSV import.

## 9. Testing and quality

- Unit tests for: bag-plus-kilo maths, stock ledger sums, discount and tax, checkout transaction (rolls back on failure), udhaar partial and full payment, account closing and reopening, over-payment rejection, duplicate customer warning, category/type filtering, import with bad rows.
- Infrastructure tests use in-memory SQLite.
- Keep ViewModels free of UI types so they are testable.
- Add a `README.md` with build, run, test and release commands.

## 10. Acceptance criteria

- App opens in English and Urdu (right-to-left) and creates its database on first run.
- All sample items import without errors; bad rows are reported.
- A 10-line bill is saved and printed in under 2 seconds; a reprint shows identical totals.
- A customer who pays the full balance disappears from the open ledger, appears under Cleared accounts, and the voucher says "ACCOUNT SETTLED".
- A new customer can be created from the billing screen and is credited in the same step.
- Search by Urdu or English name returns results under 0.5 s with 10,000 items.
- Stock on hand always equals the sum of movements.
- Restoring a backup returns the exact earlier data.

## 11. Build phases (do them in order; stop after each and report)

1. Solution skeleton, central package versions, DI host, Serilog, SQLite + migrations, localization with Urdu RTL, shell window with navigation, login.
2. Catalogue: categories, types, items, units, bag-plus-kilo `Quantity`, Excel/CSV import, seed data.
3. Stock ledger, low-stock and expiry alerts, stock count screen.
4. Billing, payments, receipt rendering and printing (80 mm and A5), reprint.
5. Customers and the udhaar ledger: vouchers, auto-close on full payment, cleared history, new customer from billing.
6. Purchases and supplier ledger, reports with PDF and Excel export, backup and restore, audit log.
7. Installer and update channel with Velopack, user manual (Urdu and English), final test pass.

## 12. UI design system (mandatory for every new feature)

HassanTraders has one visual identity: deep navy sidebar, blue-teal primary (`#0B6FA4`), clean neutral surfaces, semantic status colours. It lives in `Resources/` (tokens and styles) and `Common/Controls/` (reusable controls) and is documented in [docs/design-system.md](docs/design-system.md). **Read that file before writing or changing any XAML.**

When you build or change a screen:

1. **Colours** come only from `Brush.*` resources (`{DynamicResource Brush.Primary}`). Raw hex values exist only in `Resources/Themes/Colors.Light.xaml` and `Colors.Dark.xaml`. Never use named colours (`White`, `Red`). Do not add a colour unless a genuine new state needs it; if so, add it to **both** theme files, add its brush to `Brushes.xaml`, and document it.
2. **Text** uses the `Text.*` styles (`Text.Page`, `Text.Section`, `Text.Body`, `Text.Secondary`, `Text.Caption`, `Text.Label`, `Text.Number`, `Text.Kpi`). Do not set `FontSize`, `FontWeight` or `Foreground` ad hoc.
3. **Spacing, size and shape** use the tokens (`Pad.Page`, `Pad.Card`, `Space.*`, `Radius.*`, `Size.*`). Use multiples of 4 px.
4. **Controls**: use the implicit styles (plain `Button`, `TextBox`, `PasswordBox`, `ComboBox`, `CheckBox`, `DataGrid`, `ProgressBar`) and the keyed variants (`Button.Primary`, `Button.Danger`, `Button.Ghost`, `Button.Segment`, `Button.Touch`). One `Button.Primary` per view. Prefer the shared controls: `PageHeader`, `StatCard`, `StatusPill`, `StateView`, `SearchBox`, `FormField`, `StatusBanner`, and `IDialogService` (never `MessageBox`).
5. **Page layout**: root `Grid` with `Margin="{DynamicResource Pad.Page}"`, a `PageHeader`, then `Card` surfaces. List screens copy `Features/Items/Views/ItemsView.xaml`; the dashboard is the pattern for overview screens; settings is the pattern for forms.
6. **Every data screen has all its states**: loading, ready, empty, no-match (if filterable) and error with retry, using `StateView` and the `ViewState` enum.
7. **No fake data.** Show real numbers or an honest empty state (`StateView` saying the module arrives later). Never invent statistics or placeholder rows.
8. **Both languages, equally polished.** All text via `{wpf:T Key}` or `ILocalizationService` with keys in both `.resx` files. Layout must work mirrored right-to-left. Numbers use `Text.Number` (tabular, always left-to-right digits). Test the screen in English and Urdu, light and dark, at 1366x768.
9. **Do not redesign existing screens** when adding a feature. Reuse; extend the design system only when a real need appears, and then update `docs/design-system.md` in the same change.
10. The tests in `DesignSystemTests` enforce points 1 and 8 (no raw colours, resource keys exist, contrast, translations). They must pass.

## Follow-up prompts

- "Next phase. Do Phase N from CLAUDE.md. Keep to the architecture rules. Run all tests before you report."
- "Review the code so far against section 4 (architecture rules) and section 10 (acceptance criteria). List any rule that is broken and fix it."
