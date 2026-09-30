# Viva Voce & Presentation Guide: Restaurant Table Reservation & Food POS System

**Project 09:** Restaurant Table Reservation & Food POS System  
**Target Framework:** ASP.NET Core (.NET 10.0) with Entity Framework Core & SQL Server LocalDB  
**Evaluation Criteria Maximum Marks:** 30 / 30  

---

## 1. Quick Demonstration & Login Credentials

### Seeded Credentials:
| Account Role | Email Address | Password | Purpose |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin@restaurant.com` | `Admin@123` | Full access to Admin Area, POS Live Order board, LINQ dynamic demand reports, Table management, and Menu CRUD. |
| **Customer** | `customer@restaurant.com` | `Customer@123` | Customer ordering, session cart, pre-booking dining tables, payment simulation, and order tracking. |

> **Pro Presentation Shortcut:** On the Login page (`/Account/Login`), two 1-click shortcut buttons (**Demo Admin** & **Demo Customer**) are provided so you can instantly log in and present without manually typing credentials!

---

## 2. Evaluation Criteria & Architecture Explanations

### Criterion 1: Project Architecture & Layout Design (05 Marks)
* **Folder Structure:**
  - `Areas/Admin/`: Dedicated MVC Area for administration (`DashboardController`, `ReportsController`, `OrdersController`, `MenuItemsController`, `TablesController`, `ReservationsController`).
  - `Controllers/`: Public and Customer controllers (`HomeController`, `MenuController`, `CartController`, `ReservationController`, `CheckoutController`, `OrderController`, `AccountController`).
  - `Data/`: `RestaurantDbContext.cs` and `DbInitializer.cs` for automated migration and rich seeding.
  - `Models/Entities/` & `Models/ViewModels/`: Clean separation of database domain models and view presentation models.
  - `Services/`: Business logic layer (`CartService`, `PaymentGatewayService`, `ReportService`).
  - `Helpers/`: Custom Tag Helpers (`<status-badge status="..." />`) and HTML helper extensions (`@Html.FormatCurrency(...)`, `@Html.VegNonVegBadge(...)`).
* **Layout Design:**
  - Master layout `_Layout.cshtml` with Bootstrap 5.3 + Bootstrap Icons.
  - Luxury restaurant palette: amber/gold accents, dark charcoal slate, glassmorphic cards, and hover zoom animations on dishes.
  - Strongly-typed Partial Views: `_MenuItemCardPartial.cshtml`, `_MenuItemListPartial.cshtml`, `_OrderReceiptPartial.cshtml`.

---

### Criterion 2: Model Validations, Security & Navigation (05 Marks)
* **Client & Server-Side Data Annotations:**
  - All ViewModels (`RegisterViewModel`, `LoginViewModel`, `TableBookingViewModel`, `CheckoutViewModel`, `MenuItem`) feature comprehensive data annotations: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[RegularExpression]`, `[DataType]`.
  - Client-side validation: Handled instantly using `jquery.validate.js` and `jquery.validate.unobtrusive.js`.
  - Server-side validation: Enforced in controllers with `ModelState.IsValid`.
* **Authentication & Role-Based Authorization:**
  - Configured in `Program.cs` via Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`).
  - Passwords hashed using ASP.NET Core's cryptographically secure `PasswordHasher<User>`.
  - Policies and attributes: `[Authorize(Roles = "Admin")]` protecting all admin controllers.
* **Custom Routing & Parameter Constraints:**
  - Regex slug constraint: `Menu/Category/{categorySlug:regex(^[a-zA-Z0-9-]+$)}`
  - Numeric minimum constraint: `Reservation/Table/{tableId:int:min(1)}`
  - Regex order code constraint: `Order/Receipt/{orderNumber:regex(^ORD-[0-9]{4}-[0-9]{4,6}$)}`
  - MVC Areas route: `{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`

---

### Criterion 3: Database & Entity Framework Integration (06 Marks)
* **DbContext & DbSets:**
  - `RestaurantDbContext` configured with `(localdb)\ProjectModels` and database `RestaurantPosDb`.
  - Relational mapping with Fluent API:
    - `Category` 1:N `MenuItem` (Cascade delete)
    - `DiningTable` 1:N `TableReservation`
    - `DiningTable` 0..1:N `Order` (Dine-In table link)
    - `Order` 1:N `OrderItem` 
    - `MenuItem` 1:N `OrderItem` (Restrict delete)
    - `Order` 1:1 `PaymentTransaction`
    - `Role` 1:N `User`
* **Complete CRUD Implementation:**
  - Menu Items: Create, Read (search & category filters), Update, Delete.
  - Dining Tables: Create, Read, Update, Delete.
  - Reservations & Orders: Complete workflow management and status updating.

---

### Criterion 4: LINQ Data Processing & Dynamic Reports (06 Marks)
Located in `Services/ReportService.cs` and displayed on `/Admin/Reports`:

#### 1. Top-Selling Food Items (LINQ Query Syntax)
Joins orders, items, menu items, and categories; groups by dish; aggregates quantities and revenue:
```csharp
var query = from order in ordersQuery
            join item in _context.OrderItems on order.OrderId equals item.OrderId
            join menuItem in _context.MenuItems on item.MenuItemId equals menuItem.MenuItemId
            join category in _context.Categories on menuItem.CategoryId equals category.CategoryId
            where (!categoryId.HasValue || category.CategoryId == categoryId.Value)
            group new { item, menuItem, category } by new
            {
                menuItem.MenuItemId,
                menuItem.Name,
                CategoryName = category.Name,
                menuItem.Price,
                menuItem.IsVegetarian,
                menuItem.ImageUrl
            } into g
            select new TopSellingItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                ItemName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                UnitPrice = g.Key.Price,
                TotalQuantitySold = g.Sum(x => x.item.Quantity),
                TotalRevenue = g.Sum(x => x.item.LineTotal),
                IsVegetarian = g.Key.IsVegetarian
            };
```

#### 2. Peak Hour Sales Volume Analysis (LINQ Method Syntax)
Groups orders by `OrderDate.Hour` to pinpoint lunchtime rush (12:30-2:30 PM) and dinnertime rush (7:30-10:00 PM):
```csharp
var hourlyMap = dayOrders
    .GroupBy(o => o.Hour)
    .ToDictionary(
        g => g.Key,
        g => new {
            Count = g.Count(),
            Volume = g.Sum(x => x.TotalAmount)
        }
    );
```

#### 3. Table Turnover Report (LINQ Projection)
Calculates reservations, orders served, daily turnover frequency, and estimated occupancy:
```csharp
var report = tables.Select(t => {
    var tableReservations = reservations.Where(r => r.TableId == t.TableId).ToList();
    var tableOrders = orders.Where(o => o.TableId == t.TableId).ToList();
    var totalServed = tableOrders.Count + tableReservations.Count;
    return new TableTurnoverDto {
        TableNumber = t.TableNumber,
        Capacity = t.Capacity,
        TotalReservations = tableReservations.Count,
        TotalOrdersServed = tableOrders.Count,
        TotalRevenueGenerated = tableOrders.Sum(o => o.TotalAmount),
        TurnoverRatePerDay = Math.Round(totalServed / totalDays, 1)
    };
}).OrderByDescending(x => x.TotalRevenueGenerated).ToList();
```

---

### Criterion 5: Shopping Cart, Payment Gateway & AJAX Integration (04 Marks)
* **Persistent Session State Cart:**
  - Backed by ASP.NET Core `ISession` (`HttpContext.Session.SetObjectAsJson` / `GetObjectFromJson`).
  - Real-time navbar item count badge updated via AJAX.
  - Cart survives page reloads and browser tab switching.
* **Simulated Payment Gateway:**
  - Multi-method checkout: Credit/Debit Cards, UPI (with QR preview & VPA), NetBanking.
  - Generates verifiable `TransactionReference` (`TXN-...`), authorization code, and payment timestamp.
  - Produces printable **Tax Invoice** with 5% GST (2.5% CGST + 2.5% SGST breakdown).
* **Unobtrusive AJAX:**
  - Adding dishes to cart triggers an animated toast and updates badge without page reload.
  - Menu filtering (categories, search, veg-only switch) fetches partial HTML asynchronously (`/Menu/FilterAjax`).
  - POS order status transitions update instantaneously.
  - Table availability check checks bookings on date/slot change.

---

### Criterion 6: Working Site Demo & Viva Voce (04 Marks)
* Zero runtime crash errors; defensive error handling; automated seeding on first launch.
* 6 realistic categories, 18+ menu dishes with Unsplash photos, 12 tables, past orders across different hours for immediate rich graphical reporting.
