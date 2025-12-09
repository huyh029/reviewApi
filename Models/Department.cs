namespace reviewApi.Models
{
    public class Department
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? ParentCode { get; set; }
        public Department? Parent { get; set; }
        public ICollection<Department>? Children { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<EvaluationFlowDepartment>? EvaluationFlowDepartments { get; set; }
    }
}
