using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure EF Core with SQL Server (LocalDB ProjectModels)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure Cookie Authentication & Role-Based Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "RestaurantManagement_AuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

// 3. Configure Session State (Syllabus Requirement: Session State)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "RestaurantManagement_Session";
});

builder.Services.AddHttpContextAccessor();

// 4. Register Custom Dependency Injection Services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 5. Add Controllers and Razor Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Automatic Database Migration / Creation & Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RestaurantDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing and seeding the database.");
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 7. Custom Routing with Parameter Constraints (Syllabus: MVC Areas & Custom Routing with parameter constraints)
// Route Constraint: regex for menu category slug
app.MapControllerRoute(
    name: "menuByCategory",
    pattern: "Menu/Category/{categorySlug:regex(^[a-zA-Z0-9-]+$)}",
    defaults: new { controller = "Menu", action = "Category" });

// Route Constraint: int and min for table booking
app.MapControllerRoute(
    name: "tableReservationRoute",
    pattern: "Reservation/Table/{tableId:int:min(1)}",
    defaults: new { controller = "Reservation", action = "BookForTable" });

// Route Constraint: regex for order receipt matching ORD-XXXX
app.MapControllerRoute(
    name: "orderReceiptRoute",
    pattern: "Order/Receipt/{orderNumber:regex(^ORD-[0-9]{{4}}-[0-9]{{4,6}}$)}",
    defaults: new { controller = "Order", action = "Receipt" });

// 8. MVC Areas Route Configuration (Syllabus: Area routing for Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 9. Default MVC Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
