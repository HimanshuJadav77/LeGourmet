using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public interface IReportService
    {
        Task<List<TopSellingItemDto>> GetTopSellingFoodItemsAsync(DateTime? fromDate, DateTime? toDate, int? categoryId);
        Task<List<PeakHourSalesDto>> GetPeakHourSalesVolumeAsync(DateTime? date);
        Task<List<TableTurnoverDto>> GetTableTurnoverReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<DashboardKpisDto> GetDashboardKPIsAsync();
    }
}
