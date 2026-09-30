using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Helpers;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public class CartService : ICartService
    {
        private const string CartSessionKey = "RestaurantCart_Session_State";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RestaurantDbContext _dbContext;

        public CartService(IHttpContextAccessor httpContextAccessor, RestaurantDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available in the current context.");

        public CartViewModel GetCart()
        {
            var cart = Session.GetObjectFromJson<CartViewModel>(CartSessionKey);
            if (cart == null)
            {
                cart = new CartViewModel();
                Session.SetObjectAsJson(CartSessionKey, cart);
            }
            return cart;
        }

        public void AddToCart(int menuItemId, int quantity, string? notes)
        {
            if (quantity <= 0) return;

            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    existingItem.Notes = notes;
                }
            }
            else
            {
                var menuItem = _dbContext.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefault(m => m.MenuItemId == menuItemId);

                if (menuItem != null)
                {
                    cart.Items.Add(new CartItemViewModel
                    {
                        MenuItemId = menuItem.MenuItemId,
                        Name = menuItem.Name,
                        CategoryName = menuItem.Category?.Name,
                        Price = menuItem.Price,
                        Quantity = quantity,
                        ImageUrl = menuItem.ImageUrl,
                        IsVegetarian = menuItem.IsVegetarian,
                        Notes = notes
                    });
                }
            }

            Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public void UpdateQuantity(int menuItemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                Session.SetObjectAsJson(CartSessionKey, cart);
            }
        }

        public void RemoveFromCart(int menuItemId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item != null)
            {
                cart.Items.Remove(item);
                Session.SetObjectAsJson(CartSessionKey, cart);
            }
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        public void SetTable(int? tableId, string? tableNumber)
        {
            var cart = GetCart();
            cart.SelectedTableId = tableId;
            cart.TableNumber = tableNumber;
            Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public void SetOrderType(string orderType)
        {
            var cart = GetCart();
            cart.OrderType = orderType;
            Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public int GetTotalItemCount()
        {
            return GetCart().TotalItems;
        }
    }
}
