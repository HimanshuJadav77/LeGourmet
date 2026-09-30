using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;
using Restaurant_Management.Models.ViewModels;
using System.Security.Claims;

namespace Restaurant_Management.Controllers
{
    public class ReservationController : Controller
    {
        private readonly RestaurantDbContext _context;

        public ReservationController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int? tableId)
        {
            var model = new TableBookingViewModel
            {
                ReservationDate = DateTime.Today,
                TimeSlot = "19:30 - 21:00",
                GuestCount = 2,
                TableId = tableId ?? 0
            };

            // Pre-fill user data if authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email;
                    model.CustomerPhone = user.PhoneNumber ?? "";
                }
            }

            model.AvailableTables = await GetAvailableTablesAsync(model.ReservationDate, model.TimeSlot, model.GuestCount);

            if (tableId.HasValue && model.AvailableTables.Any(t => t.TableId == tableId.Value))
            {
                model.TableId = tableId.Value;
            }
            else if (model.AvailableTables.Any())
            {
                model.TableId = model.AvailableTables.First().TableId;
            }

            return View(model);
        }

        // Custom route endpoint: Reservation/Table/{tableId:int:min(1)}
        [HttpGet]
        public async Task<IActionResult> BookForTable(int tableId)
        {
            return await Book(tableId);
        }

        // Unobtrusive AJAX endpoint for dynamic table filtering
        [HttpGet]
        public async Task<IActionResult> CheckAvailabilityAjax(DateTime date, string timeSlot, int guestCount)
        {
            var tables = await GetAvailableTablesAsync(date, timeSlot, guestCount);
            return Json(tables.Select(t => new
            {
                tableId = t.TableId,
                tableNumber = t.TableNumber,
                capacity = t.Capacity,
                location = t.Location,
                status = t.Status
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(TableBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTables = await GetAvailableTablesAsync(model.ReservationDate, model.TimeSlot, model.GuestCount);
                return View(model);
            }

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                userId = user?.UserId;
            }

            var reservation = new TableReservation
            {
                TableId = model.TableId,
                UserId = userId,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                ReservationDate = model.ReservationDate.Date,
                TimeSlot = model.TimeSlot,
                GuestCount = model.GuestCount,
                SpecialRequests = model.SpecialRequests,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };

            _context.TableReservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your table reservation has been successfully confirmed!";
            return RedirectToAction(nameof(Confirmation), new { id = reservation.ReservationId });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _context.TableReservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var model = new ReservationConfirmationViewModel
            {
                ReservationId = reservation.ReservationId,
                CustomerName = reservation.CustomerName,
                CustomerEmail = reservation.CustomerEmail,
                CustomerPhone = reservation.CustomerPhone,
                TableNumber = reservation.Table?.TableNumber ?? "T-Pending",
                Location = reservation.Table?.Location ?? "Main Dining",
                Capacity = reservation.Table?.Capacity ?? reservation.GuestCount,
                ReservationDate = reservation.ReservationDate,
                TimeSlot = reservation.TimeSlot,
                GuestCount = reservation.GuestCount,
                SpecialRequests = reservation.SpecialRequests,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };

            return View(model);
        }

        public async Task<IActionResult> MyBookings(string? email, string? phone)
        {
            var query = _context.TableReservations
                .Include(r => r.Table)
                .AsQueryable();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                query = query.Where(r => r.CustomerEmail == userEmail || (r.User != null && r.User.Email == userEmail));
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(r => r.CustomerEmail.ToLower() == email.Trim().ToLower());
            }
            else if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(r => r.CustomerPhone.Contains(phone.Trim()));
            }
            else
            {
                return View(new List<TableReservation>());
            }

            var list = await query.OrderByDescending(r => r.ReservationDate).ThenBy(r => r.TimeSlot).ToListAsync();
            return View(list);
        }

        private async Task<List<DiningTable>> GetAvailableTablesAsync(DateTime date, string timeSlot, int guestCount)
        {
            var targetDate = date.Date;

            // Find tables already booked for the given date and time slot
            var bookedTableIds = await _context.TableReservations
                .Where(r => r.ReservationDate == targetDate
                         && r.TimeSlot == timeSlot
                         && r.Status != "Cancelled")
                .Select(r => r.TableId)
                .ToListAsync();

            return await _context.DiningTables
                .Where(t => t.IsActive
                         && t.Capacity >= guestCount
                         && !bookedTableIds.Contains(t.TableId))
                .OrderBy(t => t.Capacity)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();
        }
    }
}
