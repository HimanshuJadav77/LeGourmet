using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Management.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(80)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Cover Image URL")]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }

    public class MenuItem
    {
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        [Required(ErrorMessage = "Dish name is required")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Dish name must be between 2 and 120 characters")]
        [Display(Name = "Dish Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1.0, 10000.0, ErrorMessage = "Price must be between 1.00 and 10,000.00")]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Vegetarian")]
        public bool IsVegetarian { get; set; } = true;

        [Display(Name = "Spicy Level (0-3)")]
        [Range(0, 3)]
        public int SpicyLevel { get; set; } = 0;

        [Display(Name = "Available in POS/Menu")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Preparation Time (Mins)")]
        [Range(1, 120)]
        public int PreparationTimeMinutes { get; set; } = 15;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
