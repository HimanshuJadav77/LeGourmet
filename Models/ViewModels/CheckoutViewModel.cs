using System.ComponentModel.DataAnnotations;
using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Order type is required")]
        [Display(Name = "Dining Option")]
        public string OrderType { get; set; } = "Dine-In"; // Dine-In or Takeaway

        [Display(Name = "Dining Table")]
        public int? TableId { get; set; }

        [StringLength(300)]
        [Display(Name = "Kitchen Notes / Dietary Requests")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "Please select a payment gateway method")]
        [Display(Name = "Payment Gateway")]
        public string PaymentMethod { get; set; } = "Credit/Debit Card"; // Credit/Debit Card, UPI Payment, NetBanking

        // Card simulated fields
        [Display(Name = "Cardholder Name")]
        public string? CardholderName { get; set; }

        [RegularExpression(@"^4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}$", ErrorMessage = "Please enter a valid card number")]
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "MM/YY format required")]
        [Display(Name = "Expiry (MM/YY)")]
        public string? CardExpiry { get; set; }

        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "3 or 4 digit CVV")]
        [Display(Name = "CVV")]
        public string? CardCvv { get; set; }

        // UPI simulated fields
        [RegularExpression(@"^[\w.-]+@[\w.-]+$", ErrorMessage = "Enter valid UPI ID (e.g. user@okhdfcbank)")]
        [Display(Name = "UPI ID / VPA")]
        public string? UpiId { get; set; }

        // NetBanking simulated fields
        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        public CartViewModel Cart { get; set; } = new();
        public List<DiningTable> AvailableTables { get; set; } = new();
    }
}
