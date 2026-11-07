# Software requirements

The full requirements, feature IDs (F1 to F24), data model and acceptance criteria are in [CLAUDE.md](../CLAUDE.md). This file tracks status.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Skeleton, DI host, Serilog, SQLite + migrations, localization (Urdu RTL), shell + navigation, login, forced password change, audit of logins | Built, tests pass; WPF screens need a run on Windows |
| 2 | Catalogue, units, `Quantity`, import, seed items | In progress: domain model, SQLite migration, CSV/XLSX import service and sample seed are implemented; item-management UI and category/type maintenance screens remain |
| 3 | Stock ledger, alerts, stock count | Not started |
| 4 | Billing, payments, receipts, printing | Not started |
| 5 | Customers, udhaar ledger, vouchers | Not started |
| 6 | Purchases, reports, backup/restore, audit screen | Not started |
| 7 | Installer, updates, manual, final pass | Not started |
