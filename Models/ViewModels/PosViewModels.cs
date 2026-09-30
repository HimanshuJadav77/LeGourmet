using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Models.ViewModels
{
    public class PosDashboardViewModel
    {
        public List<Order> ActiveOrders { get; set; } = new();
        public List<DiningTable> DiningTables { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
        public int PlacedCount => ActiveOrders.Count(o => o.OrderStatus == "Placed");
        public int PreparingCount => ActiveOrders.Count(o => o.OrderStatus == "Preparing");
        public int ReadyCount => ActiveOrders.Count(o => o.OrderStatus == "Ready");
    }

    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}
