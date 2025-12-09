using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using reviewApi.DTO;
using reviewApi.Service;
using reviewApi.Service.Repositories;
namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/evaluations")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationService _iEvaluationService;
        public EvaluationController(IEvaluationService iEvaluationService)
        {
            _iEvaluationService = iEvaluationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetEvaluations([FromQuery] string? tab, [FromQuery] int page, [FromQuery] int limit, [FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? period, [FromQuery] int? year)
        {
            try
            {
                var data = await _iEvaluationService.GetEvaluations(tab, page, limit, search, status, period, year);
                return Ok(new
                {
                    data = data.data,
                    pagination = data.pagination
                });
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
        public async Task<TemplateDTO> getTemplate()
        {
            return await _iEvaluationService.GetTemplates(null);
        }
        [HttpGet("{id:int}")]
        public async Task<TemplateDTO> getById([FromRoute] int id)
        {
            return await _iEvaluationService.GetTemplates(id);
        }
        [HttpPost]
        public async Task<IActionResult> CreatEvaluation([FromBody] TemplateDTO payload)
        {
            
            try
            {
                await _iEvaluationService.CreatEvaluation(payload);
                return Ok(new { message = "Thêm mới đánh giá thành công!" });
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
        [HttpDelete]
        public async Task<IActionResult> DeleteEvaluations([FromBody] DeleteMultiple<int> payload)
        {
            try
            {
                await _iEvaluationService.deleteEvaluations(payload);
                return Ok(new { message = "Xóa đánh giá thành công" });
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
        [HttpPost("create-and-submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateAndSubmitEvaluation([FromForm] CreateSubmitEvaluationDTO payload, IFormFileCollection? files)
        {
            try
            {
                var data = await _iEvaluationService.CreateAndSubmitEvaluation(payload, files);
                return Ok(data);
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
        public async Task<IActionResult> UpdateEvaluation([FromRoute] int id, [FromBody] EvaluationDTO payload)
        {
            try
            {
                var data = await _iEvaluationService.UpdateEvaluation(id, payload.Scores, payload.evaluationPeriod, payload.ManagerScores);
                return Ok(data);
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

        [HttpPost("{id}/submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitEvaluation([FromRoute] int id, [FromForm] SubmitEvaluationDTO payload, IFormFileCollection? files)
        {
            try
            {
                var data = await _iEvaluationService.SubmitEvaluation(id, payload, files);
                return Ok(data);
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

        [HttpPost("{id}/withdraw")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> WithdrawEvaluation([FromRoute] int id, [FromForm] string? reason, IFormFileCollection? files)
        {
            try
            {
                var data = await _iEvaluationService.WithdrawEvaluation(id, reason, files);
                return Ok(data);
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

        [HttpPost("{id}/return")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReturnEvaluation([FromRoute] int id, [FromForm] string? reason, IFormFileCollection? files)
        {
            try
            {
                var data = await _iEvaluationService.ReturnEvaluation(id, reason, files);
                return Ok(data);
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

        [HttpPost("{id}/forward")]
        public async Task<IActionResult> ForwardEvaluation([FromRoute] int id)
        {
            try
            {
                var data = await _iEvaluationService.ForwardEvaluation(id);
                return Ok(data);
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

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteEvaluation([FromRoute] int id)
        {
            try
            {
                var data = await _iEvaluationService.CompleteEvaluation(id);
                return Ok(data);
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
        [HttpGet("{id}/chat")]
        public async Task<IActionResult> GetChat([FromRoute] int id)
        {
            try
            {
                var data = await _iEvaluationService.GetChatMessages(id);
                return Ok(data);
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

        [HttpPost("{id}/chat")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostChat([FromRoute] int id, [FromForm] string? Message, [FromForm] int? ReplyToMessageId, IFormFileCollection? files)
        {
            try
            {
                var data = await _iEvaluationService.SendChatMessage(id, Message, ReplyToMessageId, files);
                return Ok(data);
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

        [HttpGet("chat-files/{id:int}")]
        public IActionResult DownloadChatFile([FromRoute] int id)
        {
            var fileInfo = _iEvaluationService.GetChatFileById(id);
            if (fileInfo == null) return NotFound(new { message = "Không tìm thấy tệp đính kèm" });

            var filePath = fileInfo.Item1;
            var downloadName = fileInfo.Item2;
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Tệp không tồn tại trên máy chủ" });
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(downloadName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType, downloadName);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportEvaluations([FromQuery] string format, [FromQuery] string tab, [FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? period, [FromQuery] int? year)
        {
            try
            {
                var bytes = await _iEvaluationService.ExportEvaluations(format, tab, search, status, period, year);
                var fileName = format switch
                {
                    "xlsx" => "danh-sach-danh-gia.xlsx",
                    "docx" => "danh-sach-danh-gia.docx",
                    "pdf" => "danh-sach-danh-gia.pdf",
                    _ => "danh-sach-danh-gia.html"
                };
                var contentType = format switch
                {
                    "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "pdf" => "application/pdf",
                    _ => "text/html"
                };
                return File(bytes, contentType, fileName);
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
    }
}
