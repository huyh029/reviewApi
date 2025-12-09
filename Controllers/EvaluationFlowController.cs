using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;
using reviewApi.Service;
using reviewApi.Service.Repositories;
namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/evaluation-flows")]
    public class EvaluationFlowController: ControllerBase
    {
        private readonly IEvaluationFlowService _iEvaluationFlowService;
        public EvaluationFlowController(IEvaluationFlowService iEvaluationFlowService)
        {
            _iEvaluationFlowService = iEvaluationFlowService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEvaluationFlows([FromQuery] string? search, [FromQuery] int page, [FromQuery] int limit)
        {
            var data = await _iEvaluationFlowService.getEvaluationFlows(search,page,limit);
            return Ok(new
            {
                data = data.data,
                pagination = data.pagination
            });
        }
        [HttpGet("{code}")]
        public async Task<IActionResult> GetEvaluationFlowByCode([FromRoute] string code)
        {
            var data = await _iEvaluationFlowService.getById(code);
            return Ok(data);
        }
        [HttpPost]
        public async Task<IActionResult> creatEvaluationFlow([FromBody] EvaluationFlowDetailDTO payload)
        {
            await _iEvaluationFlowService.creatEvaluationFlow(payload);
            return Ok(new {message = "Tạo mới dữ liệu thành công" });
        }
        [HttpPut]
        public async Task<IActionResult> editEvaluationFlow([FromBody] EvaluationFlowDetailDTO payload)
        {
            try
            {
                await _iEvaluationFlowService.editEvaluationFlow(payload);
                return Ok(new { message = "Cập nhật dữ liệu thành công" });
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
        [HttpDelete("{code}")]
        public async Task<IActionResult> deleteEvalutionFlow([FromRoute] string code)
        {
            await _iEvaluationFlowService.deleteEvalutionFlow(code);
            return Ok(new { message = "Xóa luồng đánh giá thành công!" });
        }
        [HttpDelete]
        public async Task<IActionResult> deleteEvalutionFlows([FromBody] DeleteMultiple<string> payload)
        {
            await _iEvaluationFlowService.deleteEvalutionFlows(payload);
            return Ok(new { message = "Xóa luồng đánh giá thành công!" });
        }
    }
}
