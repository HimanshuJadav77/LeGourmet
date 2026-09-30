using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Restaurant_Management.Helpers
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("status")]
        public string? Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            var badgeClass = "badge rounded-pill ";
            var iconClass = "";
            var text = Status ?? "Unknown";

            switch (text.ToLowerInvariant())
            {
                case "available":
                    badgeClass += "bg-success text-white";
                    iconClass = "bi-check-circle-fill me-1";
                    break;
                case "reserved":
                    badgeClass += "bg-warning text-dark";
                    iconClass = "bi-calendar-check-fill me-1";
                    break;
                case "occupied":
                    badgeClass += "bg-danger text-white";
                    iconClass = "bi-person-fill me-1";
                    break;
                case "placed":
                    badgeClass += "bg-info text-dark";
                    iconClass = "bi-bell-fill me-1";
                    break;
                case "preparing":
                    badgeClass += "bg-warning text-dark";
                    iconClass = "bi-fire me-1";
                    break;
                case "ready":
                    badgeClass += "bg-primary text-white";
                    iconClass = "bi-check-all me-1";
                    break;
                case "served":
                case "completed":
                    badgeClass += "bg-success text-white";
                    iconClass = "bi-patch-check-fill me-1";
                    break;
                case "cancelled":
                    badgeClass += "bg-secondary text-white";
                    iconClass = "bi-x-circle-fill me-1";
                    break;
                case "confirmed":
                    badgeClass += "bg-success text-white";
                    iconClass = "bi-check-lg me-1";
                    break;
                case "pending":
                    badgeClass += "bg-warning text-dark";
                    iconClass = "bi-clock-fill me-1";
                    break;
                case "seated":
                    badgeClass += "bg-primary text-white";
                    iconClass = "bi-cup-hot-fill me-1";
                    break;
                default:
                    badgeClass += "bg-secondary text-white";
                    break;
            }

            output.Attributes.SetAttribute("class", badgeClass + " px-3 py-2 fw-semibold");
            output.Content.SetHtmlContent($"<i class=\"bi {iconClass}\"></i> {text}");
        }
    }
}
