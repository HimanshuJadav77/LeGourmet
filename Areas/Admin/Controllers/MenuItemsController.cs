using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        private readonly RestaurantDbContext _context;

        public MenuItemsController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(s) || (m.Description != null && m.Description.ToLower().Contains(s)));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(), "CategoryId", "Name", categoryId);
            ViewBag.SelectedCategory = categoryId;
            ViewBag.CurrentSearch = search;

            var items = await query.OrderBy(m => m.Category.DisplayOrder).ThenBy(m => m.Name).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(), "CategoryId", "Name");
            return View(new MenuItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                menuItem.CreatedAt = DateTime.UtcNow;
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dish '{menuItem.Name}' successfully added to the menu!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(), "CategoryId", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(), "CategoryId", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem)
        {
            if (id != menuItem.MenuItemId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Dish '{menuItem.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MenuItems.AnyAsync(e => e.MenuItemId == menuItem.MenuItemId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(), "CategoryId", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            ViewBag.OrderCount = await _context.OrderItems.CountAsync(oi => oi.MenuItemId == id);

            return View(menuItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.MenuItemId == id);
                if (hasOrders)
                {
                    // Soft delete / archive to preserve historical sales & receipts
                    menuItem.IsAvailable = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Dish '{menuItem.Name}' is part of past order history. It has been deactivated/archived from the menu and POS instead.";
                }
                else
                {
                    _context.MenuItems.Remove(menuItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Dish '{menuItem.Name}' has been permanently deleted.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
