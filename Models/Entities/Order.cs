using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Management.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty; // e.g. ORD-2026-0001

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public int? TableId { get; set; }
        public virtual DiningTable? Table { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Order Type")]
        public string OrderType { get; set; } = "Dine-In"; // Dine-In, Takeaway, Online

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Order Time")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "GST (5%)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Grand Total")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Order Workflow Status")]
        public string OrderStatus { get; set; } = "Placed"; // Placed, Preparing, Ready, Served, Completed, Cancelled

        [Required]
        [StringLength(100)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = "Guest";

        [Phone]
        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string? CustomerPhone { get; set; }

        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }

        [StringLength(300)]
        [Display(Name = "Kitchen / Special Instructions")]
        public string? OrderNotes { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; } = null!;

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
    }

    public class PaymentTransaction
    {
        public int TransactionId { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Display(Name = "Transaction ID")]
        public string TransactionReference { get; set; } = string.Empty; // e.g. TXN-20260913-ABCD

        [Required]
        [StringLength(40)]
        [Display(Name = "Payment Gateway Method")]
        public string PaymentMethod { get; set; } = "Credit/Debit Card"; // Card, UPI, NetBanking, Cash at POS

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Paid Amount")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        [Display(Name = "Gateway Status")]
        public string Status { get; set; } = "Completed"; // Completed, Pending, Failed, Refunded

        [StringLength(10)]
        public string? CardLastFour { get; set; }

        [StringLength(80)]
        public string? UpiId { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(100)]
        public string? GatewayAuthCode { get; set; }
    }
}
