using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface IDashboardService
    {
        Task<DashboardStatsDTO> GetStats();
        Task<DashboardFiltersDTO> GetFilters();
        Task<ChartDataDTO> GetCharts(int year, int criteriaId);
        Task<List<int>> GetYearsForCriteria(int criteriaId);
    }
}
