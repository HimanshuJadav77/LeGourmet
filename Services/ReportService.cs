using Microsoft.EntityFrameworkCore;
using Restaurant_Management.Data;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public class ReportService : IReportService
    {
        private readonly RestaurantDbContext _context;

        public ReportService(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// REPORT 1: Top-Selling Food Items
        /// Implements LINQ Query Syntax with multi-table joins, grouping, and projection.
        /// Filterable by date range and category.
        /// </summary>
        public async Task<List<TopSellingItemDto>> GetTopSellingFoodItemsAsync(DateTime? fromDate, DateTime? toDate, int? categoryId)
        {
            var ordersQuery = _context.Orders.AsNoTracking().Where(o => o.OrderStatus != "Cancelled");

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= end);
            }

            // LINQ Query Syntax for Multi-table Relational Join & Aggregation
            var query = from order in ordersQuery
                        join item in _context.OrderItems on order.OrderId equals item.OrderId
                        join menuItem in _context.MenuItems on item.MenuItemId equals menuItem.MenuItemId
                        join category in _context.Categories on menuItem.CategoryId equals category.CategoryId
                        where (!categoryId.HasValue || category.CategoryId == categoryId.Value)
                        group new { item, menuItem, category } by new
                        {
                            menuItem.MenuItemId,
                            menuItem.Name,
                            CategoryName = category.Name,
                            menuItem.Price,
                            menuItem.IsVegetarian,
                            menuItem.ImageUrl
                        } into g
                        select new TopSellingItemDto
                        {
                            MenuItemId = g.Key.MenuItemId,
                            ItemName = g.Key.Name,
                            CategoryName = g.Key.CategoryName,
                            UnitPrice = g.Key.Price,
                            TotalQuantitySold = g.Sum(x => x.item.Quantity),
                            TotalRevenue = g.Sum(x => x.item.LineTotal),
                            IsVegetarian = g.Key.IsVegetarian,
                            ImageUrl = g.Key.ImageUrl
                        };

            var results = await query.OrderByDescending(x => x.TotalQuantitySold).ToListAsync();

            // Calculate percentage of total revenue
            var grandTotal = results.Sum(x => x.TotalRevenue);
            if (grandTotal > 0)
            {
                foreach (var item in results)
                {
                    item.PercentageOfTotalRevenue = Math.Round((double)(item.TotalRevenue / grandTotal) * 100, 1);
                }
            }

            return results;
        }

        /// <summary>
        /// REPORT 2: Peak Hour Sales Volume Analysis
        /// Implements LINQ Method Syntax for hourly aggregation and grouping.
        /// Maps 24 hours of the day to detect lunch rush (12-2 PM) and dinner rush (7-10 PM).
        /// </summary>
        public async Task<List<PeakHourSalesDto>> GetPeakHourSalesVolumeAsync(DateTime? date)
        {
            var targetDate = (date ?? DateTime.Today).Date;
            var endDate = targetDate.AddDays(1);

            // Fetch orders for the date range
            var dayOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate >= targetDate && o.OrderDate < endDate)
                .Select(o => new { o.OrderDate.Hour, o.TotalAmount })
                .ToListAsync();

            // LINQ Method Syntax: Grouping by Hour and aggregating metrics
            var hourlyMap = dayOrders
                .GroupBy(o => o.Hour)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Count = g.Count(),
                        Volume = g.Sum(x => x.TotalAmount)
                    }
                );

            var peakReports = new List<PeakHourSalesDto>();

            // Generate full 24-hour timeline from 10:00 AM to 11:00 PM for standard restaurant operation
            for (int h = 10; h <= 23; h++)
            {
                var count = hourlyMap.TryGetValue(h, out var stat) ? stat.Count : 0;
                var volume = hourlyMap.TryGetValue(h, out var st) ? st.Volume : 0m;
                var isPeak = (h >= 12 && h <= 14) || (h >= 19 && h <= 21);

                var ampm = h >= 12 ? "PM" : "AM";
                var displayHour = h % 12 == 0 ? 12 : h % 12;
                var label = $"{displayHour}:00 {ampm}";

                peakReports.Add(new PeakHourSalesDto
                {
                    HourOfDay = h,
                    TimeLabel = label,
                    TotalOrders = count,
                    TotalSalesVolume = volume,
                    IsPeakHour = isPeak
                });
            }

            return peakReports;
        }

        /// <summary>
        /// REPORT 3: Table Turnover Report
        /// Implements LINQ Method Syntax with GroupJoin and Projections.
        /// Calculates table occupancy, total parties served, and average revenue per table.
        /// </summary>
        public async Task<List<TableTurnoverDto>> GetTableTurnoverReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);
            var totalDays = Math.Max(1, (end - start).TotalDays);

            var tables = await _context.DiningTables.AsNoTracking().ToListAsync();

            var reservations = await _context.TableReservations
                .AsNoTracking()
                .Where(r => r.ReservationDate >= start && r.ReservationDate < end && r.Status != "Cancelled")
                .ToListAsync();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.TableId != null && o.OrderDate >= start && o.OrderDate < end && o.OrderStatus != "Cancelled")
                .ToListAsync();

            // LINQ Projection for Table Turnover Metrics
            var report = tables.Select(t =>
            {
                var tableReservations = reservations.Where(r => r.TableId == t.TableId).ToList();
                var tableOrders = orders.Where(o => o.TableId == t.TableId).ToList();

                var totalServed = tableOrders.Count + tableReservations.Count;
                var turnoverPerDay = Math.Round(totalServed / totalDays, 1);
                var revenue = tableOrders.Sum(o => o.TotalAmount);
                // Estimated occupancy percentage based on standard 8-hour service day
                var occupancy = Math.Min(100.0, Math.Round((turnoverPerDay * 1.5 / 8.0) * 100, 1));

                return new TableTurnoverDto
                {
                    TableId = t.TableId,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    Location = t.Location,
                    Status = t.Status,
                    TotalReservations = tableReservations.Count,
                    TotalOrdersServed = tableOrders.Count,
                    TotalRevenueGenerated = revenue,
                    TurnoverRatePerDay = turnoverPerDay,
                    AverageOccupancyRate = occupancy
                };
            }).OrderByDescending(x => x.TotalRevenueGenerated).ToList();

            return report;
        }

        /// <summary>
        /// Overall Dashboard KPIs for quick administrative snapshot
        /// </summary>
        public async Task<DashboardKpisDto> GetDashboardKPIsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.OrderStatus != "Cancelled")
                .ToListAsync();

            var activeOrdersCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.OrderStatus == "Placed" || o.OrderStatus == "Preparing" || o.OrderStatus == "Ready");

            var todayReservationsCount = await _context.TableReservations
                .AsNoTracking()
                .CountAsync(r => r.ReservationDate >= today && r.ReservationDate < tomorrow && r.Status != "Cancelled");

            var availableTablesCount = await _context.DiningTables
                .AsNoTracking()
                .CountAsync(t => t.Status == "Available" && t.IsActive);

            var totalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderStatus != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            var totalMenuItems = await _context.MenuItems
                .AsNoTracking()
                .CountAsync(m => m.IsAvailable);

            return new DashboardKpisDto
            {
                TodayRevenue = todayOrders.Sum(o => o.TotalAmount),
                TotalRevenue = totalRevenue,
                TodayOrdersCount = todayOrders.Count,
                ActiveOrdersCount = activeOrdersCount,
                TodayReservationsCount = todayReservationsCount,
                AvailableTablesCount = availableTablesCount,
                TotalMenuItemsCount = totalMenuItems
            };
        }
    }
}
