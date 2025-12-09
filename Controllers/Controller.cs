using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;
using reviewApi.DTO;
namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IService _iService;
        public RoleController(
           IService iService
        )
        {
            _iService = iService;
        }
        [HttpGet("predefined")]
        public async Task<List<RoleDTO>> GetRoles()
        {
            return await _iService.getRoles();
        }
    }
    [ApiController]
    [Route("api/departments")]
    public class DepartmentController : ControllerBase
    {
        private readonly IService _iService;
        public DepartmentController(
           IService iService
        )
        {
            _iService = iService;
        }
        [HttpGet]
        public async Task<List<DepartmentDTO>> GetDepartments()
        {
            return await _iService.getDepartments();
        }
    }
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IService _iService;
        public UserController(IService iService)
        {
            _iService = iService;
        }
        [HttpGet("managers")]
        public async Task<List<UserDTO>> GetManagers()
        {
            return await _iService.getManagers();
        }
    }
}
