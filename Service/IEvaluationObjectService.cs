using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface IEvaluationObjectService
    {
        Task<responseDTO<List<EvaluationObjectsDTO>>> getObjects(string? search, int limit, int page);
        Task postObject(EvaluationObjectsDTO payload);
        Task putObject(EvaluationObjectsDTO payload);
        Task deleteObject(string code);
        Task<List<DepartmentNodeDTO>> getEvaluationObjectTree(string? search);
        Task putRolesInEvaluationObject(List<assigmentDTO> assignments);
        Task<List<EvaluationObjectsDTO>> activeList();
    }
}
