using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TablesController : Controller
    {
        private readonly RestaurantDbContext _context;

        public TablesController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? location, string? status)
        {
            var query = _context.DiningTables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(location) && location != "All")
            {
                query = query.Where(t => t.Location == location);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(t => t.Status == status);
            }

            var tables = await query.OrderBy(t => t.TableNumber).ToListAsync();

            ViewBag.Locations = await _context.DiningTables.Select(t => t.Location).Distinct().ToListAsync();
            ViewBag.CurrentLocation = location ?? "All";
            ViewBag.CurrentStatus = status ?? "All";

            return View(tables);
        }

        public IActionResult Create()
        {
            return View(new DiningTable { Status = "Available", Capacity = 4, Location = "Main Dining" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiningTable table)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.DiningTables.AnyAsync(t => t.TableNumber.ToLower() == table.TableNumber.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("TableNumber", "A table with this number already exists.");
                    return View(table);
                }

                _context.DiningTables.Add(table);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Table {table.TableNumber} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var table = await _context.DiningTables.FindAsync(id);
            if (table == null) return NotFound();

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiningTable table)
        {
            if (id != table.TableId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(table);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Table {table.TableNumber} updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.DiningTables.AnyAsync(e => e.TableId == table.TableId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _context.DiningTables.FindAsync(id);
            if (table != null)
            {
                var hasActiveOrders = await _context.Orders.AnyAsync(o => o.TableId == id && (o.OrderStatus == "Placed" || o.OrderStatus == "Preparing" || o.OrderStatus == "Ready"));
                if (hasActiveOrders)
                {
                    TempData["ErrorMessage"] = $"Cannot delete table {table.TableNumber} because it currently has active POS orders!";
                    return RedirectToAction(nameof(Index));
                }

                var hasHistory = await _context.TableReservations.AnyAsync(r => r.TableId == id) || await _context.Orders.AnyAsync(o => o.TableId == id);
                if (hasHistory)
                {
                    table.IsActive = false;
                    table.Status = "Maintenance";
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Table {table.TableNumber} is part of past booking/order history. It has been deactivated from floor layout instead.";
                }
                else
                {
                    _context.DiningTables.Remove(table);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Table {table.TableNumber} was permanently deleted.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
