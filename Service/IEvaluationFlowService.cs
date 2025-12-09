using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface IEvaluationFlowService
    {
        Task<responseDTO<List<EvaluationFlowDTO>>> getEvaluationFlows(string? search,int page, int limit);
        Task<EvaluationFlowDetailDTO> getById(string code);
        Task creatEvaluationFlow(EvaluationFlowDetailDTO payload);
        Task editEvaluationFlow(EvaluationFlowDetailDTO payload);
        Task deleteEvalutionFlow(string code);
        Task deleteEvalutionFlows(DeleteMultiple<string> payload);
    }
}
