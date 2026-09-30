using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly RestaurantDbContext _context;

        public OrdersController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, string? search)
        {
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.PaymentTransaction)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(o => o.OrderNumber.ToLower().Contains(s)
                                      || o.CustomerName.ToLower().Contains(s)
                                      || (o.CustomerPhone != null && o.CustomerPhone.Contains(s)));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentSearch = search;
            ViewBag.ActiveCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Placed" || o.OrderStatus == "Preparing" || o.OrderStatus == "Ready");

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.PaymentTransaction)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Unobtrusive AJAX Endpoint: Advance order workflow status without reloading
        [HttpPost]
        public async Task<IActionResult> UpdateStatusAjax([FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null || request.OrderId <= 0)
            {
                return Json(new { success = false, message = "Invalid request parameters." });
            }

            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.OrderStatus = request.NewStatus;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                orderId = order.OrderId,
                newStatus = order.OrderStatus,
                message = $"Order {order.OrderNumber} status advanced to {order.OrderStatus}."
            });
        }
    }
}
