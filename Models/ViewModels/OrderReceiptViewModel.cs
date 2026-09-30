using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Models.ViewModels
{
    public class OrderReceiptViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string? TableNumber { get; set; }
        public string? TableLocation { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? OrderNotes { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxRate => 0.05m;
        public decimal CGST => Math.Round(SubTotal * 0.025m, 2);
        public decimal SGST => Math.Round(SubTotal * 0.025m, 2);
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new();

        public PaymentTransactionViewModel? Payment { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public bool IsVegetarian { get; set; }
    }

    public class PaymentTransactionViewModel
    {
        public string TransactionReference { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CardLastFour { get; set; }
        public string? UpiId { get; set; }
        public string? GatewayAuthCode { get; set; }
    }
}
