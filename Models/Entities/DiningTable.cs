using System.ComponentModel.DataAnnotations;

namespace Restaurant_Management.Models.Entities
{
    public class DiningTable
    {
        [Key]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Table number is required")]
        [StringLength(20)]
        [Display(Name = "Table Number")]
        public string TableNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seating capacity is required")]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20 guests")]
        [Display(Name = "Seating Capacity")]
        public int Capacity { get; set; } = 4;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(50)]
        [Display(Name = "Dining Area")]
        public string Location { get; set; } = "Main Dining"; // Main Dining, Window View, Romantic Booth, Garden Patio, VIP Lounge

        [Required]
        [StringLength(30)]
        [Display(Name = "Current Status")]
        public string Status { get; set; } = "Available"; // Available, Reserved, Occupied, Maintenance

        public bool IsActive { get; set; } = true;

        public virtual ICollection<TableReservation> Reservations { get; set; } = new List<TableReservation>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class TableReservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Table is required")]
        [Display(Name = "Table")]
        public int TableId { get; set; }
        public virtual DiningTable Table { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        [StringLength(30)]
        [Display(Name = "Dining Time Slot")]
        public string TimeSlot { get; set; } = "19:00 - 20:30";

        [Required(ErrorMessage = "Guest count is required")]
        [Range(1, 20, ErrorMessage = "Guest count must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int GuestCount { get; set; } = 2;

        [StringLength(500)]
        [Display(Name = "Special Requests / Dietary Needs")]
        public string? SpecialRequests { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Reservation Status")]
        public string Status { get; set; } = "Confirmed"; // Pending, Confirmed, Seated, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
