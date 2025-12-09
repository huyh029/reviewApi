using Microsoft.AspNetCore.Http;
using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface ICriteriaSetService
    {
        Task<responseDTO<List<CriteriaSetDTO>>> getCriteriaSets(int page,int limit, string? search,string? _object,int? year,int? month);
        Task creatCriteriaSets(CriteriaSetDTO payload);
        Task<CriteriaSetDTO> getById(int id);
        Task updateCriteriaSet(CriteriaSetDTO payload);
        Task<byte[]> GenerateExcel();
        Task<byte[]> ExportCriteriaSets(string format, string? search, string? _object, int? year, int? month);
        Task<byte[]> GenerateTemplate();
        Task ImportCriteriaSets(IFormFile file);
        Task<byte[]> GenerateCriteriaTemplate();
        Task ImportCriteriaFromTemplate(int criteriaSetId, IFormFile file);
        Task<List<CriteriaNodeDTO>> PreviewCriteriaFromTemplate(IFormFile file);
        Task deleteCriteriaSet(int id);
        Task deleteCriteriaSets(DeleteMultiple<int> payload);
        Task<int> LookupCriteriaSet(string period, string year);
    }
}
