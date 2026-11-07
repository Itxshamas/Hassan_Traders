# Hassan Traders design system

The single source of truth for how every screen looks and behaves. If this file and a screen disagree, fix the screen. If a screen needs something this file does not offer, extend the design system first (and update this file in the same change), then use it.

**Identity:** a deep navy sidebar and title bar, a blue-teal primary for actions, clean neutral surfaces with hairline borders and soft shadows, and four semantic status colours. No gradients, no decoration for its own sake, readable in English and Urdu, light and dark.

## 1. Where things live

```text
src/HassanTraders.Desktop/
├── App.xaml                         merges WPF-UI base, the active colour file, then DesignSystem.xaml
├── Resources/
│   ├── Themes/
│   │   ├── Colors.Light.xaml        ← the ONLY files with hex colours (same keys in both)
│   │   └── Colors.Dark.xaml
│   ├── DesignSystem.xaml            entry point: merges every file below
│   └── Styles/
│       ├── Brushes.xaml             Brush.* for every colour token
│       ├── Tokens.xaml              spacing, padding, radius, sizes, type scale, icon sizes, shadows
│       ├── Typography.xaml          Text.* styles
│       ├── Buttons.xaml  Inputs.xaml  Forms.xaml  DataGrid.xaml  Cards.xaml
│       ├── Progress.xaml  Navigation.xaml  Dialogs.xaml  Pills.xaml  Icons.xaml
│       └── Converters.xaml          shared value converters
└── Common/Controls/                 reusable controls (below)
```

**Change the brand colour:** edit `Color.Primary` (and `PrimaryHover`, `PrimaryPressed`, `PrimaryTint`) in the two `Colors.*.xaml` files. Nothing else.

**How theming works:** every style refers to `Brush.*` through `DynamicResource`. `WpfThemeApplier` swaps the colour dictionary (`Colors.Light.xaml` ⇄ `Colors.Dark.xaml`), applies the WPF-UI theme and sets the WPF-UI accent to `Color.Primary`, so the whole UI follows live.

## 2. Colour

Use `{DynamicResource Brush.Name}`. Never a hex value, never a named colour.

| Token | Light | Dark | Used for |
| --- | --- | --- | --- |
| `Sidebar` | `#0B1F3A` | `#060F1C` | Sidebar, title bar, login brand panel |
| `SidebarHover` / `SidebarActive` | `#12305A` / `#1A4075` | `#0F2440` / `#163457` | Sidebar item states |
| `SidebarText` / `SidebarTextActive` | `#C9D6E8` / `#FFFFFF` | `#B4C3D8` / `#FFFFFF` | Text and icons on the sidebar |
| `SidebarAccent` | `#3FB8D9` | `#3FB8D9` | Selected-item bar, brand underline |
| `Primary` | `#0B6FA4` | `#4DA3D9` | Main actions, links, focus, selection |
| `PrimaryHover` / `PrimaryPressed` | `#095C8A` / `#07496E` | `#6BB5E2` / `#3C8DC2` | Primary button states |
| `PrimaryTint` | `#E6F2F8` | `#17324D` | Selected rows, soft icon backgrounds |
| `OnPrimary` | `#FFFFFF` | `#06223A` | Text on a primary fill |
| `AppBackground` | `#F4F6F9` | `#0A1626` | Page background |
| `Surface` / `SurfaceAlt` / `SurfaceHover` | `#FFFFFF` / `#F8FAFC` / `#F1F5F9` | `#12233A` / `#172C47` / `#1B3352` | Cards and inputs / table headers / hover |
| `Border` / `BorderStrong` | `#DDE3EB` / `#C3CCD8` | `#243A58` / `#345074` | Hairlines / input borders |
| `TextPrimary` | `#14202E` | `#E8EEF6` | Body and headings |
| `TextSecondary` | `#4A5A6D` | `#A8B6C8` | Supporting text, labels |
| `TextMuted` | `#5F6F82` | `#8A9BB0` | Captions, placeholders |
| `TextDisabled` | `#9AA7B6` | `#5E6F85` | Disabled text (exempt from contrast) |
| `Success` / `SuccessBg` | `#17784A` / `#E3F4EC` | `#3DBE82` / `#12362A` | OK, active, saved |
| `Warning` / `WarningBg` | `#9A6414` / `#FDF3DC` | `#E0A83A` / `#3A2E12` | Low, needs attention |
| `Danger` / `DangerBg` | `#C62F3E` / `#FBE6E8` | `#EF6B77` / `#3E1A20` | Out, errors, destructive |
| `DangerHover` / `OnDanger` | `#A82533` / `#FFFFFF` | `#F58A93` / `#2A0A0E` | Danger button |
| `Info` / `InfoBg` | `#34639F` / `#E8EFF9` | `#6FA0E0` / `#17284A` | Neutral information |
| `Overlay` / `Shadow` | `#990B1F3A` / `#0B1F3A` | `#B2020912` / `#000000` | Backdrops, shadow tint |

Semantic rule: pick the colour by meaning, not by taste. Green = fine, amber = watch, red = problem or destructive, blue = information. Status is never colour alone: a pill always carries a word.

Contrast: all text pairs meet WCAG AA 4.5 : 1 in both themes (a test enforces it).

## 3. Typography

| Style | Size / weight | Use |
| --- | --- | --- |
| `Text.Page` | 24 SemiBold | Page title (in `PageHeader`) |
| `Text.Section` | 18 SemiBold | Card and section titles |
| `Text.Body` | 14 | Default reading text |
| `Text.Secondary` | 14, secondary colour | Supporting text |
| `Text.Label` | 13 SemiBold, secondary | Field label above an input |
| `Text.Caption` | 12, muted | Second lines, hints, table headers |
| `Text.Kpi` | 28 SemiBold, tabular | Big numbers on `StatCard` |
| `Text.Number` | 14, tabular, right aligned | Numbers in tables |
| `Text.Error` | 13, danger | Field and form errors |

- **English:** Segoe UI Variable (fallback Segoe UI). **Urdu:** Noto Nastaliq Urdu (bundled), body 16, line height 40 (set once on the window root; secondary Urdu lines on English screens use line height 26).
- Use at most two weights on a screen (regular and semibold).
- Numbers are Latin digits everywhere so screens and receipts match. In tables use `Text.Number`: it forces left-to-right digits and right alignment, correct in both languages.

## 4. Spacing, shape, elevation

- **Grid:** 4 px. Tokens `Space.1`…`Space.8` = 4, 8, 12, 16, 20, 24, 32.
- **Page padding** `Pad.Page` (28,24). **Card padding** `Pad.Card` (20). Gap between cards: 16. Gap inside a form: 16 (set by `FormField`).
- **Radius:** controls 6, cards 10, dialogs 14, pills 11 (`Radius.*`).
- **Sizes:** controls 40 px (`Size.Control`), touch 52 px (`Size.ControlTouch`), table rows 44 px, table header 40 px, sidebar 232 px.
- **Elevation:** `Shadow.Card` for cards, `Shadow.Dialog` for dialogs and pop-ups. Nothing else.
- **Icons:** WPF-UI `SymbolIcon` (Fluent set, names like `Box24`). Sizes `Icon.Small/Medium/Large/Hero` = 16/20/24/40. Use the `Icon`, `Icon.Primary`, `Icon.Muted` styles.

## 5. Components

### Buttons
A plain `<Button>` is the Secondary style. Variants: `Button.Primary` (the main action, **one per view**), `Button.Danger` (destructive, always confirm first), `Button.Ghost` (toolbar and low emphasis), `Button.Segment` (choice in a segmented control; set `Tag="{Binding IsSelected}"`), `Button.Touch` / `Button.PrimaryTouch` (52 px for billing), `Button.Sidebar` (on the navy sidebar). Action placement: primary action trailing (right in English, left in Urdu), cancel before it.

### Inputs
Plain `TextBox`, `PasswordBox`, `ComboBox`, `CheckBox` are already styled. Always wrap in a **`FormField`** (label above, error below) or use **`SearchBox`**. Focus shows a 2 px primary border; errors a red border plus the message under the field.

### Cards
`<Border Style="{StaticResource Card}">` for content; `Card.Flush` for a table that runs edge to edge; `Divider` between sections inside a card.

### Tables (DataGrid)
A plain `DataGrid` is read-only, flat, with sortable headers, hover and selection tint. Columns: text in `DataGridTextColumn`/templates, numbers in `Text.Number`, status in `StatusPill`. Two-line cells: main line regular/semibold, second line `Text.Caption`. Give numeric columns `SortMemberPath` pointing at the raw number. Put the grid in a `Card.Flush`, filters above it in a `Card`. Search and filter in the ViewModel; replace rows with `BulkObservableCollection.ReplaceAll` (one refresh, keeps sort).

### Shared controls (`Common/Controls`)
| Control | Purpose | Key properties |
| --- | --- | --- |
| `PageHeader` | Title, optional subtitle, trailing actions | `Title`, `Subtitle`, `Actions` |
| `StatCard` | KPI tile | `Label`, `Value`, `Hint`, `IconName` |
| `StatusPill` | Rounded status word | `Text`, `Tone` (Neutral, Success, Warning, Danger, Info) |
| `StateView` | Empty, loading, error | `Kind`, `Title`, `Message`, `IconName`, `Action` |
| `SearchBox` | Search input with icon, placeholder, clear | `Text`, `Placeholder` |
| `FormField` | Label + input + error | `Label`, `Error`, content |
| `StatusBanner` | Inline success/warning/error/info message; hides when empty | `Message`, `Tone` |
| `DialogWindow` | Themed dialog; use through `IDialogService` | `ShowError`, `ShowInfo`, `Confirm(…, danger)` |

### Navigation
`ListBox` with `Nav.List`: icon + label, rounded hover, accent bar and lighter fill on the selected item. Items come from `RoleAccess`, so users only see what they may open. To add a screen: add the `Feature`, its icon in `NavItemViewModel.IconFor`, its `Nav_*` strings, a `DataTemplate` in `ShellView.xaml`, and a case in `NavigationService`.

### Dialogs
Use `IDialogService`, never `MessageBox`. Errors: title says what failed, message says what to do. Confirmations: name the action on the confirm button ("Delete", not "OK"); destructive ones pass `danger: true`. Enter confirms, Esc cancels.

## 6. Page patterns

**Every page** has a root `Grid` with `Margin="{DynamicResource Pad.Page}"`, a `PageHeader`, then cards.

- **List page** (`Features/Items/Views/ItemsView.xaml` is the reference): `PageHeader` → filter `Card` (SearchBox + ComboBoxes + "Clear filters" ghost button) → results `Card.Flush` holding the `DataGrid` and one `StateView` per non-ready state.
- **Overview page** (`DashboardView.xaml`): `StatCard` row → breakdown cards (`ProgressBar` rows) → cards for later modules showing an honest empty state.
- **Form page** (`SettingsView.xaml`, `ChangePasswordView.xaml`): one `Card` per group, `FormField`s, `StatusBanner` at the top for errors, one `Button.Primary`.
- **Sign-in** (`LoginView.xaml`): navy brand panel + centered card.

## 7. States (every data screen implements all that apply)

| State | Show | Notes |
| --- | --- | --- |
| Loading | `StateView Kind="Loading"` | Title and a short message |
| Ready | The content | |
| Empty | `StateView` with icon, title, what will appear here | Never a blank card |
| No matches | `StateView` with "Clear filters" action | Only when filters are active |
| Error | `StateView Kind="Error"` with **Try again** (`Button.Primary`) | Log the exception; never show it |
| Success/failure of an action | `StatusBanner` (inline) or a dialog for destructive confirms | |

ViewModels expose a `ViewState` and `ShowGrid/ShowLoading/…` booleans; views bind visibility with `BoolToVisibility`.

**No fake data.** Show real numbers or an honest empty state saying when the module arrives ("Available once billing is added"). Never placeholder rows or invented statistics.

## 8. Urdu and right-to-left

- All text through `{wpf:T Key}` (XAML) or `ILocalizationService` (ViewModels); keys in **both** `Strings.resx` and `Strings.ur.resx`. A test fails if either is missing.
- The window root sets `FlowDirection`, font, size and line height from the language; children inherit. **Do not** hard-code left/right alignment for meaning: `HorizontalAlignment="Left"` means "leading edge" and mirrors in Urdu. Margins and padding mirror too.
- Mirrored automatically: sidebar position, column order, button order, scroll bars, ComboBox arrow. Not mirrored (set explicitly): numbers (`Text.Number`), barcodes, and text of the other language (give it its own `FlowDirection` and font, as the item grid's second line does).
- Keep both names visible on item rows: main line in the active language, second line the other.
- Check each new screen in Urdu: nothing clipped (Nastaliq is tall), nothing overlapping when mirrored, buttons in the right order.

## 9. Responsive rules

- Minimum window 1024 × 640; designed and checked at **1366 × 768**. The sidebar is fixed at 232 px; the page area flexes.
- Use `*` columns and `MaxWidth` for forms (forms are 400 to 760 px wide, not full width). Lists fill the width. Scroll vertically with a `ScrollViewer`, never horizontally on a normal screen.
- Cards in a row use `UniformGrid` or star columns with 8 px margins each side (16 px gap).

## 10. Rules for every new feature

1. Follow the colour tokens, type styles, spacing and components above. No raw colours, no ad-hoc fonts.
2. Reuse styles and controls; do not copy-paste style blocks into a screen.
3. Keep the layout of the closest existing page (list, overview, form).
4. Do not add a colour, size or style without a genuine need; if you do, add it here, in both theme files if it is a colour, and say why in `docs/decisions.md`.
5. Do not redesign existing screens when adding a feature.
6. Implement every state (loading, empty, no-match, error) and both languages and both themes.
7. Keep styling out of ViewModels; ViewModels expose state, views decide how it looks.

## 11. What the tests enforce

`DesignSystemTests` (in `tests/HassanTraders.Tests/Desktop`) fail when: the two theme files disagree on keys; a colour lacks a brush; a hex or named colour appears outside the theme files; a `StaticResource`/`DynamicResource` key is not defined anywhere (it would crash at runtime); a style file is not merged; a size resource used for a grid column is not a `GridLength`; a `{wpf:T …}` key is missing in either language; any text/background pair drops below 4.5 : 1 contrast in either theme.

## 12. Visual check on Windows (the tests cannot see pixels)

For each of these, in **English light, English dark, Urdu light, Urdu dark** at 1366 × 768 (and once at 1024 × 640): Sign in, Change password (sign in as `admin` first time), Dashboard, Items (clear and with a search typed, and with a category chosen), Settings, one placeholder screen (for example Billing), and the sign-out and error dialogs. Look for: clipped Urdu, overlapping or mis-ordered controls when mirrored, unreadable text, wrong colours after switching theme, the title bar buttons visible on the navy bar.
