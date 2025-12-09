namespace reviewApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string DepartmentCode { get; set; }  
        public string RoleCode { get; set; }
        public string Password { get; set; }
        public Department Department { get; set; }
        public Role Role { get; set; }
        public ICollection<EvaluationObjectRole>? EvaluationObjectRoles { get; set; }
        public ICollection<Evaluation> Evaluations { get; set; }
        public ICollection<EvaluationChat>? EvaluationChats { get; set; }

    }
}
