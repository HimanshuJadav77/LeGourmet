using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;
using Restaurant_Management.Models.ViewModels;
using System.Security.Claims;

namespace Restaurant_Management.Controllers
{
    public class OrderController : Controller
    {
        private readonly RestaurantDbContext _context;

        public OrderController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Track(string? orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return View((Order?)null);
            }

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.PaymentTransaction)
                .FirstOrDefaultAsync(o => o.OrderNumber.Trim().ToLower() == orderNumber.Trim().ToLower());

            return View(order);
        }

        // Custom Route Handler with regex parameter constraint: Order/Receipt/{orderNumber}
        [HttpGet]
        public async Task<IActionResult> Receipt(string orderNumber)
        {
            return RedirectToAction("Receipt", "Checkout", new { orderNumber = orderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .Include(o => o.PaymentTransaction)
                .AsQueryable();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                query = query.Where(o => o.CustomerEmail == userEmail || (o.User != null && o.User.Email == userEmail));
            }
            else
            {
                // If not logged in, empty state with search prompt
                return View(new List<Order>());
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusAjax(string orderNumber)
        {
            var order = await _context.Orders
                .Where(o => o.OrderNumber == orderNumber)
                .Select(o => new { o.OrderNumber, o.OrderStatus, o.OrderDate })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            return Json(new { success = true, status = order.OrderStatus });
        }
    }
}
