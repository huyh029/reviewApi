using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;
using reviewApi.Service;
using reviewApi.Service.Repositories;

namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/criteria-sets")]
    public class CriteriaSetController : ControllerBase
    {
        private readonly ICriteriaSetService _iCriteriaSetService;
        public CriteriaSetController(ICriteriaSetService iCriteriaSetService)
        {
            _iCriteriaSetService = iCriteriaSetService;
        }
        [HttpGet]
        public async Task<IActionResult> getCriteriaSets([FromQuery] int page, [FromQuery] int limit, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? search, [FromQuery(Name = "object")] string? _object)
        {
            var data = await _iCriteriaSetService.getCriteriaSets(page, limit, search, _object, year, month);
            return Ok(new
            {
                data = data.data,
                pagination = data.pagination
            });
        }
        [HttpPost]
        public async Task<IActionResult> creatCriteriaSet([FromBody] CriteriaSetDTO payload)
        {

            try
            {
                if (payload == null) return BadRequest(new { message = "payload is required" });
                await _iCriteriaSetService.creatCriteriaSets(payload);
                return Ok(new { message = "Tạo mới tiêu chí đánh giá thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }
        [HttpGet("{id:int}")]
        public async Task<CriteriaSetDTO> getById([FromRoute] int id)
        {
            return await _iCriteriaSetService.getById(id);
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> lookupCriteriaSet([FromQuery] string period, [FromQuery] string year)
        {
            try
            {
                var id = await _iCriteriaSetService.LookupCriteriaSet(period, year);
                return Ok(new { criteriaSetId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> updateCriteriaSet([FromRoute] int id, [FromBody] CriteriaSetDTO payload)
        {
            try
            {
                if (payload == null) return BadRequest(new { message = "payload is required" });
                payload.id = id;
                await _iCriteriaSetService.updateCriteriaSet(payload);
                return Ok(new { message = "Cập nhật tiêu chí đánh giá thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }
        [HttpGet("template")]
        public async Task<IActionResult> getTemplate()
        {
            var fileBytes = await _iCriteriaSetService.GenerateTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "criteria-set-template.xlsx");
        }
        [HttpGet("export")]
        public async Task<IActionResult> exportCriteriaSets([FromQuery] string format, [FromQuery] string? search, [FromQuery(Name = "object")] string? _object, [FromQuery] int? month, [FromQuery] int? year)
        {
            var fileBytes = await _iCriteriaSetService.ExportCriteriaSets(format, search, _object, year, month);
            var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "xlsx" : format.ToLower();
            var contentType = normalizedFormat switch
            {
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "pdf" => "application/pdf",
                _ => "text/html"
            };
            var fileName = normalizedFormat == "pdf" ? "criteria-sets.pdf" : $"criteria-sets.{normalizedFormat}";
            return File(fileBytes, contentType, fileName);
        }
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> importCriteriaSets(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn tệp hợp lệ!" });
            await _iCriteriaSetService.ImportCriteriaSets(file);
            return Ok(new { message = "Tải lên dữ liệu thành công!" });
        }
        [HttpGet("download-template")]
        public async Task<IActionResult> downloadTemplate()
        {
            var fileBytes = await _iCriteriaSetService.GenerateTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "criteria-set-template.xlsx");
        }
        [HttpGet("criteria-template")]
        public async Task<IActionResult> downloadCriteriaTemplate()
        {
            var fileBytes = await _iCriteriaSetService.GenerateCriteriaTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "criteria-template.xlsx");
        }
        [HttpPost("{id:int}/criteria-import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> importCriteriaFromTemplate([FromRoute] int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn tệp hợp lệ!" });
            await _iCriteriaSetService.ImportCriteriaFromTemplate(id, file);
            return Ok(new { message = "Tải dữ liệu tiêu chí thành công!" });
        }
        [HttpPost("criteria-import-preview")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> previewCriteriaImport(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn tệp hợp lệ!" });
            var data = await _iCriteriaSetService.PreviewCriteriaFromTemplate(file);
            return Ok(new { data });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCriteriaSet([FromRoute] int id)
        {
            await _iCriteriaSetService.deleteCriteriaSet(id);
            return Ok(new { message = "Xóa thành công tiêu chí!" });
        }
        [HttpDelete]
        public async Task<IActionResult> deleteCriteriaSets([FromBody] DeleteMultiple<int> payload)
        {
            await _iCriteriaSetService.deleteCriteriaSets(payload);
            return Ok(new { message = "Xóa thành công các tiêu chí!" });
        }
    }
}
