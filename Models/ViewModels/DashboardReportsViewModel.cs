namespace Restaurant_Management.Models.ViewModels
{
    public class DashboardReportsViewModel
    {
        // Filter criteria
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string ActiveTab { get; set; } = "topSelling"; // topSelling, peakHours, tableTurnover

        // Report 1: Top-Selling Food Items
        public List<TopSellingItemDto> TopSellingItems { get; set; } = new();

        // Report 2: Peak Hour Sales Volume Analysis
        public List<PeakHourSalesDto> PeakHourSales { get; set; } = new();

        // Report 3: Table Turnover Report
        public List<TableTurnoverDto> TableTurnoverMetrics { get; set; } = new();

        // Overall KPIs
        public DashboardKpisDto KPIs { get; set; } = new();
    }

    public class TopSellingItemDto
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double PercentageOfTotalRevenue { get; set; }
        public bool IsVegetarian { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class PeakHourSalesDto
    {
        public int HourOfDay { get; set; } // 0-23
        public string TimeLabel { get; set; } = string.Empty; // e.g. "12:00 PM"
        public int TotalOrders { get; set; }
        public decimal TotalSalesVolume { get; set; }
        public decimal AverageOrderValue => TotalOrders > 0 ? Math.Round(TotalSalesVolume / TotalOrders, 2) : 0;
        public bool IsPeakHour { get; set; }
    }

    public class TableTurnoverDto
    {
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public int TotalReservations { get; set; }
        public int TotalOrdersServed { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public double TurnoverRatePerDay { get; set; } // Average parties served per day
        public double AverageOccupancyRate { get; set; } // Estimated occupancy %
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardKpisDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TodayOrdersCount { get; set; }
        public int ActiveOrdersCount { get; set; }
        public int TodayReservationsCount { get; set; }
        public int AvailableTablesCount { get; set; }
        public int TotalMenuItemsCount { get; set; }
    }
}
