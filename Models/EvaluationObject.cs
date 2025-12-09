namespace reviewApi.Models
{
    public class EvaluationObject
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } // e.g., "Active", "Inactive"
        public ICollection<EvaluationObjectRole>? EvaluationObjectRoles { get; set; }
        public ICollection<CriteriaSetObject>? CriteriaSetObjects { get; set; }
    }
    public class EvaluationObjectRole
    {
        public int Id { get; set; }
        public string EvaluationObjectCode { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public EvaluationObject EvaluationObject { get; set; }
    }
}
