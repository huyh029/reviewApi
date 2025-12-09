namespace reviewApi.Models
{
    public class Role
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<EvaluationFlowRole>? EvaluationFlowRoles { get; set; }
    }
}
