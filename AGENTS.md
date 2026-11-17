# AGENTS.md

Operational guide for AI agents working on this repository. Read before editing.

## Project

SportPlus — football-shirt storefront. ASP.NET Core Blazor (`.NET 11`, `net11.0`),
**static SSR only**, MySQL/MariaDB via Pomelo EF Core 9, Bootstrap 5.3 + Font Awesome.
UI copy is Vietnamese.

## Commands

```bash
dotnet build                 # expect 0 errors, 2 pre-existing NU1902 warnings
dotnet run                   # http://localhost:5277 (launchSettings http profile)
dotnet run --urls http://127.0.0.1:5199   # preferred for scripted checks
dotnet run --launch-profile https         # https://localhost:7253
mysql -uadmin -padmin blazor -e 'select ...'
```

- Kill stray servers before rebuilding: `pgrep -af '[b]lazor_app/bin' | awk '{print $1}' | xargs -r kill`.
- Database: `localhost:3306`, DB `blazor`, user/password `admin`/`admin`
  (connection string in `appsettings.json`).
- First run: `DataSeeder` calls `EnsureCreatedAsync` then seeds `Data/seed.json`
  (131 products, 392 images, 10 users, 2 orders, 2 cart items). It **skips**
  everything when `products` has rows.

## Layout

```
Program.cs                 DI, auth, pipeline, seeding, MapControllers + MapRazorComponents
Controllers/               MVC controllers for form POST endpoints (no views)
  AuthController.cs        POST auth/login, auth/register, auth/logout
  CartController.cs        POST cart/add
  OrdersController.cs      POST orders/create
Components/Pages/          Routable pages (@page), static SSR
Components/Layout/         MainLayout, Header, Footer, ReconnectModal
Interfaces/                Service contracts (IProductService, IUserService, ICartService, IOrderService)
Services/                  Implementations using IDbContextFactory<AppDbContext>
Models/                    Entities, enums, AppDbContext, search criteria/result DTOs
Utils/                     Shared formatting/display helpers (see Rules 6)
Data/DataSeeder.cs         Schema + seed loader
Data/seed.json             Seed data (copied to output)
wwwroot/theme.css          Global monochrome theme (loaded last)
wwwroot/js/cart.js         Cart select-all checkbox behavior
```

## Routes

| Method | Path | Owner |
| --- | --- | --- |
| GET | `/` | `Components/Pages/Home.razor` |
| GET | `/products` (+ `q`, `sort`, `page`, repeated `brand`, `category`, `kit_type`, `league`, `team`, `season`) | `ProductSearch.razor` |
| GET | `/products/{id:int}` | `ProductDetail.razor` |
| GET | `/cart` | `Cart.razor` |
| GET | `/payment` (+ `ids`, `done`, `error`) | `Payment.razor` |
| GET | `/orders` | `Orders.razor` |
| GET | `/orders/{id:int}` | `OrderDetail.razor` |
| GET | `/login`, `/register`, `/account` | auth/account pages |
| GET | `/error`, `/not-found` | fallback pages |
| POST | `/auth/login`, `/auth/register`, `/auth/logout` | `AuthController` |
| POST | `/cart/add` | `CartController` |
| POST | `/orders/create` | `OrdersController` |

## Rules

1. **Static SSR only.** Do not add `@rendermode`, `InteractiveServer`, or JS component
   interop. Auth state comes from `IHttpContextAccessor` + `ClaimTypes.NameIdentifier`.
2. **No DbContext in components.** Resolve data through `Interfaces/*`; implementations
   take `IDbContextFactory<AppDbContext>`, dispose per call, use `AsNoTracking` for reads.
3. **POST endpoints live in `Controllers/`.** Never add `app.MapPost(...)` to `Program.cs`.
   POST paths must not collide with Blazor page routes: use `/auth/*`, `/cart/add`,
   `/orders/create`. Controllers read `Request.Form`, return `Redirect(...)`, and do
   inline auth checks (no `[Authorize]`, no antiforgery attributes — existing forms
   carry no tokens).
4. **Custom CSS must be minimal.** Prefer Bootstrap utilities/grid. Scoped CSS only when
   Bootstrap cannot express it. Theme is monochrome: ink `#1a1a1a`, paper `#f5f5f5`,
   borders `rgba(26,26,26,.15)` (`wwwroot/theme.css`). Do not introduce semantic colors
   (blue/red/green/orange); `theme.css` overrides Bootstrap at CSS-variable level.
5. **Vietnamese UI copy**, default Bootstrap components, Font Awesome icons.
6. **Formatting helpers are shared.** Use `Utils/PriceFormatter` (vi-VN `N0` + `₫`),
   `Utils/DateFormatter` (`dd/MM/yyyy HH:mm`), `Utils/ImageUrlHelper` (leading `/`,
   SortOrder first image), `Utils/OrderDisplay` (status label/badge class, pay-method
   label), `Utils/ProductDisplay.DiscountPercent`. Never re-add local copies in pages.
7. **Search filtering is server-side** in `IProductService.SearchAsync`
   (`ProductSearchCriteria` → `ProductSearchResult`, page size 25, page clamped).
   Filter values are lookup `Code` except `season` = `Season.Name`; OR within a group,
   AND across groups.
8. **Enum persistence:** `User.PayMethod`/`User.Role` as lowercase strings
   (`online`/`cash`, `admin`/`buyer`); `CartItem.Size`/`OrderDetail.Size` as enum names
   (`S`…`XXL`); `Order.Status` lowercase `pending`/`confirmed`/`delivered`/`cancelled`.
9. **Orders** are written transactionally: `IOrderService.CreateFromCartAsync` creates
   `orders` + `order_details` from selected cart rows and deletes those rows; `Total` =
   sum of `Product.Price * Quantity`. Order reads are user-scoped.
10. **Never reset or re-seed the database.** Snapshot before destructive tests:
    `mysqldump -uadmin -padmin --no-create-info --complete-insert blazor cart_items orders order_details users > /tmp/opencode/backup.sql`.
    Restore only rows you changed; real user-created data may exist (e.g. user 13,
    order 103) and must be preserved.
11. **Naming clashes:** page class `OrderDetail` vs `Models.OrderDetail` — qualify the
    model as `blazor_app.Models.OrderDetail`. `Orders.razor` uses `OrderList` because
    `Orders` collides with the component class.
12. Passwords are stored plaintext to match seed data. Do not "fix" without an explicit
    request; mention it as a production risk.

## Verification workflow

1. `dotnet build` → 0 errors; only 2 `NU1902` (MySqlConnector 2.4.0) warnings allowed.
2. Start on a test port and probe with `curl` (check status, redirect `Location`, key
   rendered strings). No browser/Chromium is installed — JS behavior is reviewed, not
   automated.
3. For writes, verify DB state with `mysql` and restore affected rows/`AUTO_INCREMENT`.
4. Kill the server and rebuild at the end.

## Test accounts (seeded)

| Email | Password | Role |
| --- | --- | --- |
| `admin1@myshop.vn` | `Admin@123` | admin, has orders 101/102 and cart rows |
| `an.nguyen@gmail.com` | `Buyer@123` | buyer, empty cart/orders |

Useful DB checks:

```sql
SELECT Id, UserId, ProductId, Size, Quantity FROM cart_items ORDER BY Id;
SELECT Id, UserId, PayMethod, Status, Total FROM orders ORDER BY Id DESC;
SELECT Id, OrderId, ProductId, Quantity, Size, Price FROM order_details ORDER BY Id DESC;
```

## Known pitfalls

- `UseHttpsRedirection` logs "Failed to determine the https port for redirect" when run
  HTTP-only; harmless.
- `MapRazorComponents` serves page routes; POST-only paths return 404 on GET due to
  `UseStatusCodePagesWithReExecute("/not-found")`.
- `EnsureCreated` does not migrate existing schemas; a stale DB will not gain new columns.
- `Counter.razor` is unused scaffold code.
