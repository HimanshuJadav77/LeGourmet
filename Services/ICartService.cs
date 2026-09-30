using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public interface ICartService
    {
        CartViewModel GetCart();
        void AddToCart(int menuItemId, int quantity, string? notes);
        void UpdateQuantity(int menuItemId, int quantity);
        void RemoveFromCart(int menuItemId);
        void ClearCart();
        void SetTable(int? tableId, string? tableNumber);
        void SetOrderType(string orderType);
        int GetTotalItemCount();
    }
}
