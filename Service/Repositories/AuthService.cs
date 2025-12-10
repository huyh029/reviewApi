using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using reviewApi.DTO;
using reviewApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace reviewApi.Service.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            IConfiguration config,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _config = config;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }
        public async Task<ResponseLoginDTO> Login(RequestLoginDTO dto)
        {
            User u = _unitOfWork.User.Find(
                u => u.Username == dto.username
                && u.Password == dto.password
            ).FirstOrDefault();
            if (u == null)
            {
                throw new Exception("Tài khoản đăng nhập không đúng");
            }
            else
            {
                return new ResponseLoginDTO
                {
                    token = GenerateJwtToken(u.Id ,u.FullName,u.RoleCode,u.DepartmentCode)
                };
            }
        }

        public async Task Logout()
        {
            var User = _httpContextAccessor.HttpContext?.User;

            if (User == null)
                throw new Exception("not found");

            var jti = User.FindFirst("jti")?.Value;
            var exp = User.FindFirst("exp")?.Value;

            if (jti == null || exp == null)
                throw new Exception("fail");
            var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
            _cache.Set($"blacklist_{jti}", true, expTime);

        }

        public async Task Register(RequestRegisterDTO dto)
        {
            _unitOfWork.User.Add(
                new User
                {
                    Username = dto.Username,
                    Password = dto.Password,
                    RoleCode = dto.RoleCode,
                    DepartmentCode = dto.DepartmentCode
                }
            );
            _unitOfWork.Complete();
        }
        public string GenerateJwtToken(int userId,string name,string role, string department)
        {
            // Fixed JWT settings to khớp với cấu hình trong Program.cs
            const string jwtKey = "supersecretkey1234567890abcdefxyz!";
            const string jwtIssuer = "uerManage";
            const string jwtAudience = "uerManage";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim("name", name),
            new Claim("userId", $"{userId}"),
            new Claim("roleId", role),
            new Claim("department", department),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
