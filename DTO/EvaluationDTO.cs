using System.Text.Json.Serialization;

namespace reviewApi.DTO
{
    public class EvaluationDTO
    {
        public int? id { get; set; }
        public int? criteriaSetId { get; set; }
        public string? fullName { get; set; }
        public string? evaluationPeriod { get; set; }
        public string? department { get; set; }
        public int? selfScore { get; set; }
        public int? managerScore { get; set; }
        public string? status { get; set; }
        public string? statusColor { get; set; }
        [JsonPropertyName("scores")]
        public Dictionary<int, int>? Scores { get; set; }
        [JsonPropertyName("managerScores")]
        public Dictionary<int, int>? ManagerScores { get; set; }
    }
    public class TemplateDTO
    {
        public int? Id { get; set; }
        public int? CriteriaSetId { get; set; }
        public string? FullName { get; set; }
        public string? EvaluationPeriod { get; set; }
        public string? Department { get; set; }
        public double? SelfScore { get; set; }
        public double? ManagerScore { get; set; }
        public string? Status { get; set; }
        public Dictionary<int, int>? Scores { get; set; } = new();
        public Dictionary<int, int>? ManagerScores { get; set; } = new();
        public string? period { get; set; }
        public int? year { get; set; }
    }

    public class CreateSubmitEvaluationDTO
    {
        public int criteriaSetId { get; set; }
        public string period { get; set; }
        public string year { get; set; }
        public Dictionary<int, int>? scores { get; set; }
        public int managerId { get; set; }
        public string? comments { get; set; }
        public string? scoresJson { get; set; }
    }

    public class SubmitEvaluationDTO
    {
        public int managerId { get; set; }
        public string? comments { get; set; }
    }
}
