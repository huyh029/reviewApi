namespace reviewApi.Models
{
    public class EvaluationFlow
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public ICollection<EvaluationFlowDepartment>? EvaluationFlowDepartments { get; set; }
        public ICollection<EvaluationFlowRole>? EvaluationFlowRoles { get; set; }
    }
    public class EvaluationFlowRole
    {
        public int Id { get; set; }
        public string EvaluationFlowCode { get; set; }
        public string RoleCode { get; set; }
        public int? ParentId { get; set; }
        public EvaluationFlowRole Parent { get; set; }
        public ICollection<EvaluationFlowRole> Child { get; set; }
        public Role Role { get; set; }
        public EvaluationFlow EvaluationFlow { get; set; }

    }
    public class EvaluationFlowDepartment
    {
        public int Id { get; set; }
        public string EvaluationFlowCode { get; set; }
        public string DepartmentCode { get; set; }
        public Department Department { get; set; }
        public EvaluationFlow EvaluationFlow { get; set; }
    }
}
