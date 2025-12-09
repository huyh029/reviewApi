namespace reviewApi.DTO
{
    public class DashboardStatsDTO
    {
        public int totalEvaluations { get; set; }
        public int pendingEvaluations { get; set; }
    }

    public class CriteriaFilterDTO
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class DashboardFiltersDTO
    {
        public List<int> years { get; set; } = new List<int>();
        public List<CriteriaFilterDTO> criteria { get; set; } = new List<CriteriaFilterDTO>();
    }

    public class PieDataDTO
    {
        public string phanLoai { get; set; }
        public double soLuong { get; set; }
    }

    public class MonthlyScoreBreakdownDTO
    {
        public string phanLoai { get; set; }
        public double diem { get; set; }
    }

    public class ChartDataDTO
    {
        public List<List<MonthlyScoreBreakdownDTO>> monthlyBreakdown { get; set; } = new();
        public List<PieDataDTO> pieData { get; set; } = new();
    }
}
