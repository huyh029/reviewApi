using Microsoft.AspNetCore.Http;
using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface IEvaluationService
    {
        Task<responseDTO<List<EvaluationDTO>>> GetEvaluations(string tab, int page, int limit, string? search, string? status, string? period, int? year);
        Task<TemplateDTO> GetTemplates(int? id);
        Task CreatEvaluation(TemplateDTO payload);
        Task<EvaluationDTO> CreateAndSubmitEvaluation(CreateSubmitEvaluationDTO payload, IFormFileCollection? files);
        Task<EvaluationDTO> UpdateEvaluation(int id, Dictionary<int, int>? scores, string? evaluationPeriod, Dictionary<int, int>? managerScores);
        Task<EvaluationDTO> SubmitEvaluation(int id, SubmitEvaluationDTO payload, IFormFileCollection? files);
        Task<EvaluationDTO> WithdrawEvaluation(int id, string? reason, IFormFileCollection? files);
        Task<EvaluationDTO> ReturnEvaluation(int id, string? reason, IFormFileCollection? files);
        Task<EvaluationDTO> ForwardEvaluation(int id);
        Task<EvaluationDTO> CompleteEvaluation(int id);
        Task deleteEvaluations(DeleteMultiple<int> payload);
        Task<List<ChatMessageDTO>> GetChatMessages(int evaluationId);
        Task<ChatMessageDTO> SendChatMessage(int evaluationId, string? message, int? replyToMessageId, IFormFileCollection? files);
        Tuple<string, string>? GetChatFileById(int id);
        Task<byte[]> ExportEvaluations(string format, string tab, string? search, string? status, string? period, int? year);
    }
}
