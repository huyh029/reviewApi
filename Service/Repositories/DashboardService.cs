using reviewApi.DTO;
using reviewApi.Models;

namespace reviewApi.Service.Repositories
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        public DashboardService(IUnitOfWork iUnitOfWork)
        {
            _iUnitOfWork = iUnitOfWork;
        }

        public async Task<DashboardStatsDTO> GetStats()
        {
            var evaluations = _iUnitOfWork.Evaluation.GetAll().ToList();
            var totalCompleted = evaluations.Count(e => string.Equals(e.Status, "Hoàn thành", StringComparison.OrdinalIgnoreCase));
            var pending = evaluations.Count(e =>
                string.Equals(e.Status, "Chờ đánh giá", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.Status, "Chờ đánh giá của thủ trưởng", StringComparison.OrdinalIgnoreCase));
            return new DashboardStatsDTO
            {
                totalEvaluations = totalCompleted,
                pendingEvaluations = pending
            };
        }

        public async Task<DashboardFiltersDTO> GetFilters()
        {
            var years = _iUnitOfWork.Evaluation.GetAll()
                .Select(e => e.PeriodYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            var criteria = _iUnitOfWork.CriteriaSet.GetAll().Select(c => new CriteriaFilterDTO
            {
                id = c.Id,
                name = c.Name
            }).ToList();
            return new DashboardFiltersDTO
            {
                years = years,
                criteria = criteria
            };
        }

        public async Task<List<int>> GetYearsForCriteria(int criteriaId)
        {
            // Lấy các năm áp dụng từ bảng CriteriaSetPeriod của bộ tiêu chí
            var years = _iUnitOfWork.CriteriaSetPeriod
                .Find(p => p.CriteriaSetId == criteriaId)
                .Select(p => p.year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            // Nếu chưa có cấu hình kỳ, fallback từ bảng Evaluation (đã hoàn thành)
            if (!years.Any())
            {
                years = _iUnitOfWork.Evaluation
                    .Find(e => e.CriteriaSetId == criteriaId && string.Equals(e.Status, "Hoàn thành", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.PeriodYear)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();
            }
            return years;
        }

        public async Task<ChartDataDTO> GetCharts(int year, int criteriaId)
        {
            // Lấy đánh giá theo năm và bộ tiêu chí
            var evals = _iUnitOfWork.Evaluation.Find(e =>
                e.PeriodYear == year &&
                e.CriteriaSetId == criteriaId &&
                (e.Status ?? "").ToLower() == "hoàn thành").ToList();
            if (!evals.Any())
            {
                return new ChartDataDTO
                {
                    monthlyBreakdown = Enumerable.Range(1, 12).Select(_ => new List<MonthlyScoreBreakdownDTO>()).ToList(),
                    pieData = new List<PieDataDTO>()
                };
            }

            var scores = _iUnitOfWork.EvaluationScore.GetAll().ToList();
            var criteria = _iUnitOfWork.Criteria.GetAll().Where(c => c.CriteriaSetId == criteriaId).ToList();
            var classifications = _iUnitOfWork.Classification.Find(c => c.CriteriaSetId == criteriaId).ToList();

            // Dictionary không chấp nhận key null, dùng -1 làm key cho root
            var childrenMap = criteria
                .GroupBy(c => c.parentId ?? -1)
                .ToDictionary(g => g.Key, g => g.ToList());

            int CalcManagerRootScore(int evaluationId)
            {
                var evalScores = scores.Where(s => s.EvaluationId == evaluationId)
                    .ToDictionary(s => s.CriteriaId, s => s.ManagerScore ?? 0);

                int GetSubtreeScore(Criteria node)
                {
                    var own = evalScores.ContainsKey(node.Id) ? evalScores[node.Id] : 0;
                    var children = childrenMap.ContainsKey(node.Id) ? childrenMap[node.Id] : new List<Criteria>();
                    var childrenScore = children.Sum(child => GetSubtreeScore(child));
                    var total = own + childrenScore;
                    return (node.TypeScore ?? string.Empty).ToLower() == "subtractive" ? -total : total;
                }

                var roots = criteria.Where(c => c.parentId == null).ToList();
                return roots.Sum(r => GetSubtreeScore(r));
            }

            // Biểu đồ cột: tổng điểm tự chấm theo tháng (gộp theo phân loại)
            var monthly = Enumerable.Range(1, 12).Select(m =>
            {
                var monthEvals = evals.Where(e => e.PeriodMonth == m).ToList();
                var monthBreakdown = new List<MonthlyScoreBreakdownDTO>();
                foreach (var cls in classifications)
                {
                    var totalScore = monthEvals
                        .Select(ev => CalcManagerRootScore(ev.Id))
                        .Where(total => total >= cls.Min && total <= cls.Max)
                        .Sum();
                    monthBreakdown.Add(new MonthlyScoreBreakdownDTO { phanLoai = cls.Name, diem = totalScore });
                }
                return monthBreakdown;
            }).ToList();

            // Biểu đồ tròn: phân loại theo Classification
            var pie = new List<PieDataDTO>();
            foreach (var cls in classifications)
            {
                var count = evals.Count(e =>
                {
                    var total = CalcManagerRootScore(e.Id);
                    return total >= cls.Min && total <= cls.Max;
                });
                pie.Add(new PieDataDTO { phanLoai = cls.Name, soLuong = count });
            }

            return new ChartDataDTO
            {
                monthlyBreakdown = monthly,
                pieData = pie
            };
        }
    }
}
