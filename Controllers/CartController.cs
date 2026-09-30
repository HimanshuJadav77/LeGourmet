using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.ViewModels;
using Restaurant_Management.Services;

namespace Restaurant_Management.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly RestaurantDbContext _context;

        public CartController(ICartService cartService, RestaurantDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.AvailableTables = await _context.DiningTables
                .Where(t => t.IsActive && t.Status == "Available")
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCartAjax([FromBody] AddToCartRequest request)
        {
            if (request == null || request.MenuItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid menu item selected." });
            }

            _cartService.AddToCart(request.MenuItemId, request.Quantity, request.Notes);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                message = "Dish added to your order cart!",
                totalItems = cart.TotalItems,
                subTotal = cart.SubTotal,
                taxAmount = cart.TaxAmount,
                grandTotal = cart.GrandTotal
            });
        }

        [HttpPost]
        public IActionResult UpdateQuantityAjax([FromBody] UpdateCartRequest request)
        {
            if (request == null || request.MenuItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            _cartService.UpdateQuantity(request.MenuItemId, request.Quantity);
            var cart = _cartService.GetCart();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == request.MenuItemId);

            return Json(new
            {
                success = true,
                itemLineTotal = item?.LineTotal ?? 0,
                quantity = item?.Quantity ?? 0,
                totalItems = cart.TotalItems,
                subTotal = cart.SubTotal,
                taxAmount = cart.TaxAmount,
                grandTotal = cart.GrandTotal
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCartAjax(int menuItemId)
        {
            _cartService.RemoveFromCart(menuItemId);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                message = "Item removed from cart.",
                totalItems = cart.TotalItems,
                subTotal = cart.SubTotal,
                taxAmount = cart.TaxAmount,
                grandTotal = cart.GrandTotal
            });
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetBadgeCount()
        {
            return Json(new { count = _cartService.GetTotalItemCount() });
        }
    }
}
