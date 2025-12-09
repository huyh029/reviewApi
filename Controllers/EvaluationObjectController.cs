using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;
using reviewApi.Service;
using reviewApi.Service.Repositories;

namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/evaluation-objects")]
    public class EvaluationObjectController : ControllerBase
    {
        private readonly IEvaluationObjectService _iEvaluationObjectService;
        public EvaluationObjectController(IEvaluationObjectService iEvaluationObjectService)
        {
            _iEvaluationObjectService = iEvaluationObjectService;
        }
        [HttpGet]
        public async Task<IActionResult> getObjects([FromQuery] string? search, [FromQuery] int limit, [FromQuery] int page)
        {

            var data = await _iEvaluationObjectService.getObjects(search, limit, page);
            return Ok(new
            {
                data = data.data,
                pagination = data.pagination
            });
        }
        [HttpPost]
        public async Task<IActionResult> postObject([FromBody] EvaluationObjectsDTO payload)
        {
            await _iEvaluationObjectService.postObject(payload);
            return Ok(new { message = "Thêm vai trò đánh giá thành công!" });
        }
        [HttpPut]
        public async Task<IActionResult> putObject([FromBody] EvaluationObjectsDTO payload)
        {
            await _iEvaluationObjectService.putObject(payload);
            return Ok(new { message = "Chỉnh sửa vai trò đánh giá thành công!" });
        }
        [HttpDelete("{code}")]
        public async Task<IActionResult> deleteObject(string code)
        {
            await _iEvaluationObjectService.deleteObject(code);
            return Ok(new { message = "Xóa thành công đối tượng" });
        }
        [HttpGet("active-list")]
        public async Task<IActionResult> getActiveList()
        {
            var data = await _iEvaluationObjectService.activeList();
            return Ok(data);
        }
    }
    [Authorize]
    [ApiController]
    [Route("api/organization-tree")]
    public class OrganizationTree : ControllerBase
    {
        private readonly IEvaluationObjectService _iEvaluationObjectService;
        public OrganizationTree(IEvaluationObjectService iEvaluationObjectService)
        {
            _iEvaluationObjectService = iEvaluationObjectService;
        }
        [HttpGet]
        public async Task<IActionResult> getEvaluationObjectTree([FromQuery] string? search)
        {
            var data = await _iEvaluationObjectService.getEvaluationObjectTree(search);
            return Ok(data);
        }
        [HttpPut("roles")]
        public async Task<IActionResult> putRolesInEvaluationObject([FromBody] List<assigmentDTO> payload)
        {
            await _iEvaluationObjectService.putRolesInEvaluationObject(payload);
            return Ok(new { message = "Cập nhật vai trò cho đối tượng đánh giá thành công!" });
        }

    }
}
