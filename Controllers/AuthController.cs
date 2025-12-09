using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;
using reviewApi.Service;

namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(
            IAuthService authService
        )
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ResponseLoginDTO> Login([FromBody] RequestLoginDTO dto)
        {
            return await _authService.Login(dto);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RequestRegisterDTO dto)
        {
            await _authService.Register(dto);
            return Ok("success");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _authService.Logout();
            return Ok("logout");
        }
    }
}
