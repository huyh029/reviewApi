namespace reviewApi.Models
{
    public class CriteriaSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CriteriaSetObject>? CriteriaSetObjects { get; set; }
        public ICollection<CriteriaSetPeriod>? CriteriaSetPeriods { get; set; }
        public ICollection<Criteria>? Criterias { get; set; }
        public ICollection<Classification>? Classifications { get; set; }
        public ICollection<Evaluation>? Evaluations { get; set; }
    }
    public class CriteriaSetObject
    {
        public int Id { get; set; }
        public int CriteriaSetId { get; set; }
        public string EvaluationObjectCode { get; set; }
        public CriteriaSet CriteriaSet { get; set; }
        public EvaluationObject EvaluationObject { get; set; }
    }
    public class CriteriaSetPeriod
    {
        public int Id { get; set; }
        public int CriteriaSetId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public CriteriaSet CriteriaSet { get; set; }
    }
    public class Criteria
    {
        public int Id { get; set; }
        public int CriteriaSetId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MaxScore { get; set; }
        public string TypeScore { get; set; }
        public int? parentId { get; set; }
        public Criteria? Parent { get; set; }
        public ICollection<Criteria>? Children { get; set; }
        public CriteriaSet CriteriaSet { get; set; }
        public ICollection<EvaluationScore>? EvaluationScores { get; set; }
    }
    public class Classification
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int CriteriaSetId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public CriteriaSet CriteriaSet { get; set; }
    }
}
