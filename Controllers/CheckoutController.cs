using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;
using Restaurant_Management.Models.ViewModels;
using Restaurant_Management.Services;
using System.Security.Claims;

namespace Restaurant_Management.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPaymentGatewayService _paymentService;
        private readonly RestaurantDbContext _context;

        public CheckoutController(ICartService cartService, IPaymentGatewayService paymentService, RestaurantDbContext context)
        {
            _cartService = cartService;
            _paymentService = paymentService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add delicious items before proceeding to checkout.";
                return RedirectToAction("Index", "Menu");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                OrderType = cart.OrderType ?? "Dine-In",
                TableId = cart.SelectedTableId,
                AvailableTables = await _context.DiningTables
                    .Where(t => t.IsActive && t.Status == "Available")
                    .OrderBy(t => t.TableNumber)
                    .ToListAsync()
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email;
                    model.CustomerPhone = user.PhoneNumber ?? "";
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your session expired or cart is empty.";
                return RedirectToAction("Index", "Menu");
            }

            model.Cart = cart;
            model.AvailableTables = await _context.DiningTables
                .Where(t => t.IsActive && t.Status == "Available")
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                userId = user?.UserId;
            }

            var orderNumber = $"ORD-2026-{DateTime.UtcNow:MMdd}-{new Random().Next(1000, 9999)}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                TableId = model.OrderType == "Dine-In" ? model.TableId : null,
                OrderType = model.OrderType,
                OrderDate = DateTime.UtcNow,
                SubTotal = cart.SubTotal,
                TaxAmount = cart.TaxAmount,
                TotalAmount = cart.GrandTotal,
                OrderStatus = "Placed",
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                OrderNotes = model.OrderNotes
            };

            foreach (var cartItem in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    LineTotal = cartItem.LineTotal,
                    Notes = cartItem.Notes
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Simulate Payment Gateway settlement
            await _paymentService.ProcessPaymentAsync(order, model);

            // Clear session cart after successful order placement
            _cartService.ClearCart();

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} placed and paid successfully!";
            return RedirectToAction(nameof(Receipt), new { orderNumber = order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.PaymentTransaction)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderReceiptViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                OrderType = order.OrderType,
                OrderStatus = order.OrderStatus,
                TableNumber = order.Table?.TableNumber,
                TableLocation = order.Table?.Location,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                OrderNotes = order.OrderNotes,
                SubTotal = order.SubTotal,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ItemName = oi.MenuItem?.Name ?? "Item",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal,
                    IsVegetarian = oi.MenuItem?.IsVegetarian ?? true
                }).ToList(),
                Payment = order.PaymentTransaction != null ? new PaymentTransactionViewModel
                {
                    TransactionReference = order.PaymentTransaction.TransactionReference,
                    PaymentMethod = order.PaymentTransaction.PaymentMethod,
                    Amount = order.PaymentTransaction.Amount,
                    PaymentDate = order.PaymentTransaction.PaymentDate,
                    Status = order.PaymentTransaction.Status,
                    CardLastFour = order.PaymentTransaction.CardLastFour,
                    UpiId = order.PaymentTransaction.UpiId,
                    GatewayAuthCode = order.PaymentTransaction.GatewayAuthCode
                } : null
            };

            return View(model);
        }
    }
}
