using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;

namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var data = await _dashboardService.GetStats();
            return Ok(data);
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var data = await _dashboardService.GetFilters();
            return Ok(data);
        }

        [HttpGet("years")]
        public async Task<IActionResult> GetYears([FromQuery] int criteriaId)
        {
            var years = await _dashboardService.GetYearsForCriteria(criteriaId);
            return Ok(new { years });
        }

        [HttpGet("charts")]
        public async Task<IActionResult> GetCharts([FromQuery] int year, [FromQuery] int criteriaId)
        {
            var data = await _dashboardService.GetCharts(year, criteriaId);
            return Ok(data);
        }
    }
}
