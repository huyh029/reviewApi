using DocumentFormat.OpenXml.Office2021.PowerPoint.Comment;

namespace reviewApi.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public int CriteriaSetId { get; set; }
        public int UserId { get; set; }
        public int? ManagerId { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public string Status { get; set; }
        public User? Manager { get; set; }
        public User User { get; set; }
        public CriteriaSet CriteriaSet { get; set; }
        public ICollection<EvaluationChat>? EvaluationChats { get; set; }
        public ICollection<EvaluationScore>? EvaluationScores { get; set; }

    }
    public class EvaluationChat
    {
        public int Id { get; set; }
        public int EvaluationId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ReplyToChatId { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public EvaluationChat? ReplyToChat { get; set; }
        public EvaluationChat? Replies { get; set; }
        public string? TypeChat { get; set; }
        public ICollection<EvaluationChatFile>? EvaluationChatFiles { get; set; }

        public Evaluation Evaluation { get; set; }
    }
    public class EvaluationChatFile
    {
        public int Id { get; set; }
        public int EvaluationChatId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public EvaluationChat EvaluationChat { get; set; }
    }
    public class EvaluationScore
    {
        public int Id { get; set; }
        public int EvaluationId { get; set; }
        public int CriteriaId { get; set; }
        public int? SelfScore { get; set; }
        public int? ManagerScore { get; set; }
        public Evaluation Evaluation { get; set; }
        public Criteria Criteria { get; set; }
    }
}
