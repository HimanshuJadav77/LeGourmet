using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace Restaurant_Management.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent FormatCurrency(this IHtmlHelper helper, decimal amount)
        {
            var formatted = string.Format(new CultureInfo("en-IN"), "₹{0:N2}", amount);
            return new HtmlString(formatted);
        }

        public static IHtmlContent VegNonVegBadge(this IHtmlHelper helper, bool isVegetarian)
        {
            if (isVegetarian)
            {
                return new HtmlString("<span class=\"badge bg-success-subtle text-success border border-success-subtle px-2 py-1\" title=\"Vegetarian\"><i class=\"bi bi-record-circle-fill text-success me-1\"></i> VEG</span>");
            }
            return new HtmlString("<span class=\"badge bg-danger-subtle text-danger border border-danger-subtle px-2 py-1\" title=\"Non-Vegetarian\"><i class=\"bi bi-triangle-fill text-danger me-1\"></i> NON-VEG</span>");
        }
    }
}
