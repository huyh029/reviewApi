namespace reviewApi.DTO
{
    public class EvaluationFlowDTO
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public string? status { get; set; }
        public List<string>? applicableDepartments { get; set; }
    }
    public class EvaluationFlowDetailDTO
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public string? status { get; set; }
        public List<string>? applicableDepartments { get; set; }
        public List<RoleHierarchyNodeDTO>? roleHierarchy { get; set; }
    }
    public class RoleHierarchyNodeDTO
    {
        public int? id { get; set; }
        public RoleHierarchyDataNodeDTO role { get; set; }
        public List<RoleHierarchyNodeDTO>? children { get; set; }
    }
    public class RoleHierarchyDataNodeDTO
    {
        public string? code { get; set; }
        public string? name { get; set; }
    }
}
