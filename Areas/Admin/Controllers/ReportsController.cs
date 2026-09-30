using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.ViewModels;
using Restaurant_Management.Services;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly RestaurantDbContext _context;

        public ReportsController(IReportService reportService, RestaurantDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? categoryId, string activeTab = "topSelling")
        {
            var model = new DashboardReportsViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                SelectedCategoryId = categoryId,
                ActiveTab = activeTab,
                KPIs = await _reportService.GetDashboardKPIsAsync(),
                TopSellingItems = await _reportService.GetTopSellingFoodItemsAsync(fromDate, toDate, categoryId),
                PeakHourSales = await _reportService.GetPeakHourSalesVolumeAsync(fromDate ?? DateTime.Today),
                TableTurnoverMetrics = await _reportService.GetTableTurnoverReportAsync(fromDate, toDate)
            };

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();

            return View(model);
        }

        // Web API Endpoint for Dynamic Chart.js Data
        [HttpGet]
        public async Task<IActionResult> GetReportDataJson(string reportType, DateTime? fromDate, DateTime? toDate, int? categoryId)
        {
            switch (reportType?.ToLower())
            {
                case "peakhours":
                    var peakData = await _reportService.GetPeakHourSalesVolumeAsync(fromDate ?? DateTime.Today);
                    return Json(peakData);

                case "topselling":
                    var topData = await _reportService.GetTopSellingFoodItemsAsync(fromDate, toDate, categoryId);
                    return Json(topData);

                case "tableturnover":
                    var turnoverData = await _reportService.GetTableTurnoverReportAsync(fromDate, toDate);
                    return Json(turnoverData);

                default:
                    return BadRequest("Invalid report type");
            }
        }
    }
}
