using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Controllers
{
    public class MenuController : Controller
    {
        private readonly RestaurantDbContext _context;

        public MenuController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, bool? vegOnly, string? sortBy)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var query = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(s) || (m.Description != null && m.Description.ToLower().Contains(s)));
            }

            if (vegOnly.HasValue && vegOnly.Value)
            {
                query = query.Where(m => m.IsVegetarian);
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(m => m.Price),
                "price_desc" => query.OrderByDescending(m => m.Price),
                "name" => query.OrderBy(m => m.Name),
                _ => query.OrderBy(m => m.Category.DisplayOrder).ThenBy(m => m.Price)
            };

            var items = await query.ToListAsync();

            var model = new MenuIndexViewModel
            {
                Categories = categories,
                MenuItems = items,
                SelectedCategoryId = categoryId,
                SearchQuery = search,
                VegOnly = vegOnly,
                SortBy = sortBy
            };

            return View(model);
        }

        // Custom Route Handler with regex parameter constraint: Menu/Category/{categorySlug}
        public async Task<IActionResult> Category(string categorySlug)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower().Replace(" ", "-").Replace("&", "and") == categorySlug.ToLower()
                                       || c.Name.ToLower().Contains(categorySlug.ToLower()));

            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index), new { categoryId = category.CategoryId });
        }

        // Unobtrusive AJAX Endpoint: returns partial view for dynamic filtering without page reload
        [HttpGet]
        public async Task<IActionResult> FilterAjax(int? categoryId, string? search, bool? vegOnly, string? sortBy)
        {
            var query = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(s) || (m.Description != null && m.Description.ToLower().Contains(s)));
            }

            if (vegOnly.HasValue && vegOnly.Value)
            {
                query = query.Where(m => m.IsVegetarian);
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(m => m.Price),
                "price_desc" => query.OrderByDescending(m => m.Price),
                "name" => query.OrderBy(m => m.Name),
                _ => query.OrderBy(m => m.Category.DisplayOrder).ThenBy(m => m.Price)
            };

            var items = await query.ToListAsync();
            return PartialView("_MenuItemListPartial", items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }
    }
}
