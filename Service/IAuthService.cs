using reviewApi.DTO;
namespace reviewApi.Service
{
    public interface IAuthService
    {
        Task<ResponseLoginDTO> Login(RequestLoginDTO dto);
        Task Register(RequestRegisterDTO dto);
        Task Logout();
    }
}
