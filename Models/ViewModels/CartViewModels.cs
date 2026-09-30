namespace Restaurant_Management.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
        public bool IsVegetarian { get; set; }
        public string? Notes { get; set; }

        public decimal LineTotal => Price * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public int TotalItems => Items.Sum(i => i.Quantity);

        public decimal SubTotal => Items.Sum(i => i.LineTotal);

        public decimal TaxRate => 0.05m; // 5% GST

        public decimal TaxAmount => Math.Round(SubTotal * TaxRate, 2);

        public decimal GrandTotal => SubTotal + TaxAmount;

        public int? SelectedTableId { get; set; }
        public string? TableNumber { get; set; }
        public string OrderType { get; set; } = "Dine-In";
    }

    public class AddToCartRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
    }

    public class UpdateCartRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}
