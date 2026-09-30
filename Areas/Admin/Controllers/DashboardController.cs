using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Services;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly RestaurantDbContext _context;
        private readonly IReportService _reportService;

        public DashboardController(RestaurantDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var kpis = await _reportService.GetDashboardKPIsAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(6)
                .ToListAsync();

            var upcomingReservations = await _context.TableReservations
                .Include(r => r.Table)
                .Where(r => r.ReservationDate >= DateTime.Today && r.Status != "Cancelled")
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.TimeSlot)
                .Take(5)
                .ToListAsync();

            ViewBag.KPIs = kpis;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.UpcomingReservations = upcomingReservations;

            return View();
        }
    }
}
