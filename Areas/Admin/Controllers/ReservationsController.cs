using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly RestaurantDbContext _context;

        public ReservationsController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date, string? status)
        {
            var targetDate = (date ?? DateTime.Today).Date;

            var query = _context.TableReservations
                .Include(r => r.Table)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(r => r.ReservationDate == targetDate);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            var reservations = await query
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.TimeSlot)
                .ToListAsync();

            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status ?? "All";

            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int reservationId, string newStatus)
        {
            var res = await _context.TableReservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (res != null)
            {
                res.Status = newStatus;

                // Sync table status if seated
                if (newStatus == "Seated" && res.Table != null)
                {
                    res.Table.Status = "Occupied";
                }
                else if (newStatus == "Completed" && res.Table != null)
                {
                    res.Table.Status = "Available";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Reservation #{res.ReservationId} marked as '{newStatus}'.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
