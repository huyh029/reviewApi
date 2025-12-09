using reviewApi.DTO;

namespace reviewApi.Service
{
    public interface IService
    {
        Task<List<RoleDTO>> getRoles();
        Task<List<DepartmentDTO>> getDepartments();
        Task<List<UserDTO>> getManagers();
    }
}
