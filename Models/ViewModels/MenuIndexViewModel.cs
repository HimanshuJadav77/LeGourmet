using Restaurant_Management.Models.Entities;

namespace Restaurant_Management.Models.ViewModels
{
    public class MenuIndexViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public bool? VegOnly { get; set; }
        public string? SortBy { get; set; }
    }
}
