using System.ComponentModel.DataAnnotations;
using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Models.ViewModels
{
    public class TableBookingViewModel
    {
        [Required(ErrorMessage = "Please select a dining table")]
        [Display(Name = "Select Table")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be at least 2 characters")]
        [Display(Name = "Your Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Please enter a valid contact phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please pick a reservation date")]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a dining time slot")]
        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; } = "19:00 - 20:30";

        [Required(ErrorMessage = "Please specify the number of guests")]
        [Range(1, 20, ErrorMessage = "Guest count must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int GuestCount { get; set; } = 2;

        [StringLength(500)]
        [Display(Name = "Special Requests / Celebration Note")]
        public string? SpecialRequests { get; set; }

        public List<DiningTable> AvailableTables { get; set; } = new();
    }

    public class ReservationConfirmationViewModel
    {
        public int ReservationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public DateTime ReservationDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public string? SpecialRequests { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
