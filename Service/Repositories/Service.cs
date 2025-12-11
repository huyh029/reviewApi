using reviewApi.DTO;
using reviewApi.Models;
namespace reviewApi.Service.Repositories
{
    public class Service : IService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        public Service(IUnitOfWork iUnitOfWork)
        {
            _iUnitOfWork = iUnitOfWork;
        }
        public async Task<List<DepartmentDTO>> getDepartments()
        {
            var all = _iUnitOfWork.Department.Find(d => true).ToList();
            var roots = all.Where(x => x.ParentCode == null).ToList();
            var result = roots.Select(r => MapToNodeDTO(r)).ToList();
            return result!;
        }

        public async Task<List<RoleDTO>> getRoles()
        {
            return _iUnitOfWork.Role.Find(r => true).Select(r => new RoleDTO
            {
                code = r.Code,
                name = r.Name
            }).ToList();
        }
        public async Task<List<UserDTO>> getManagers()
        {
            var currentUserId = _iUnitOfWork.userId ?? 0;
            var currentDept = _iUnitOfWork.department;
            if (currentUserId == 0 || string.IsNullOrEmpty(currentDept)) return new List<UserDTO>();

            var activeObjectCodes = _iUnitOfWork.EvaluationObject
                .Find(o => o.Status != null && o.Status.ToLower() == "active")
                .Select(o => o.Code)
                .ToList();
            if (!activeObjectCodes.Any()) return new List<UserDTO>();

            var activeFlowCodes = _iUnitOfWork.EvaluationFlow
                .Find(f => f.Status != null && f.Status.ToLower() == "active")
                .Select(f => f.Code)
                .ToList();
            if (!activeFlowCodes.Any()) return new List<UserDTO>();

            // Chỉ lấy flow áp dụng cho department hiện tại
            var deptFlowCodes = _iUnitOfWork.EvaluationFlowDepartment
                .Find(d => d.DepartmentCode == currentDept && activeFlowCodes.Contains(d.EvaluationFlowCode))
                .Select(d => d.EvaluationFlowCode)
                .Distinct()
                .ToList();
            if (!deptFlowCodes.Any()) return new List<UserDTO>();

            // 1. Đối tượng đánh giá mà user hiện tại được gán
            var userObjectCodes = _iUnitOfWork.EvaluationObjectRole.Find(r => r.UserId == currentUserId)
                .Select(r => r.EvaluationObjectCode)
                .Where(code => activeObjectCodes.Contains(code))
                .ToList();

            // 2. Đối tượng đánh giá đang được dùng trong các bộ tiêu chí
            var criteriaObjects = _iUnitOfWork.CriteriaSetObject
                .Find(c => activeObjectCodes.Contains(c.EvaluationObjectCode))
                .Select(c => c.EvaluationObjectCode)
                .Distinct()
                .ToList();
            var activeUserObjects = userObjectCodes.Intersect(criteriaObjects).ToList();
            if (!activeUserObjects.Any()) return new List<UserDTO>();

            // 3. Lọc user cùng đơn vị và có ít nhất 1 EvaluationObject thuộc nhóm activeUserObjects
            var targetUserIds = _iUnitOfWork.EvaluationObjectRole
                .Find(r => activeUserObjects.Contains(r.EvaluationObjectCode))
                .Select(r => r.UserId)
                .Distinct()
                .ToList();
            var usersSameDept = _iUnitOfWork.User.Find(u => targetUserIds.Contains(u.Id) && u.DepartmentCode == currentDept && u.Id != currentUserId).ToList();
            if (!usersSameDept.Any()) return new List<UserDTO>();

            // 4. Lấy vai trò hiện tại của user và tìm các EvaluationFlow chứa vai trò này
            var currentUser = _iUnitOfWork.User.GetById(currentUserId);
            var currentRoleCode = currentUser?.RoleCode;
            if (string.IsNullOrEmpty(currentRoleCode)) return new List<UserDTO>();

            var flowsWithCurrentRole = _iUnitOfWork.EvaluationFlowRole
                .Find(r => r.RoleCode == currentRoleCode && deptFlowCodes.Contains(r.EvaluationFlowCode))
                .Select(r => r.EvaluationFlowCode)
                .Distinct()
                .ToList();
            if (!flowsWithCurrentRole.Any()) return new List<UserDTO>();

            // 5. Lấy tất cả role con trong các flow đó bắt đầu từ nút có RoleCode == currentRoleCode
            var allRolesInFlows = _iUnitOfWork.EvaluationFlowRole.Find(r => flowsWithCurrentRole.Contains(r.EvaluationFlowCode)).ToList();
            // Chỉ group các node có ParentId, tránh null key
            var roleLookup = allRolesInFlows.Where(r => r.ParentId.HasValue)
                                            .GroupBy(r => r.ParentId!.Value)
                                            .ToDictionary(g => g.Key, g => g.ToList());
            var startNodes = allRolesInFlows.Where(r => r.RoleCode == currentRoleCode).ToList();

            var descendantRoles = new HashSet<string>();
            foreach (var start in startNodes)
            {
                CollectChildRoles(start.Id, roleLookup, descendantRoles);
            }

            if (!descendantRoles.Any()) return new List<UserDTO>();

            // 6. Lọc user theo role nằm trong descendantRoles
            var roleCodes = usersSameDept.Select(u => u.RoleCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var roleMap = _iUnitOfWork.Role.Find(r => roleCodes.Contains(r.Code)).ToDictionary(r => r.Code, r => r.Name);
            var managers = usersSameDept
                .Where(u => !string.IsNullOrEmpty(u.RoleCode) && descendantRoles.Contains(u.RoleCode))
                .Select(u => new UserDTO
                {
                    id = u.Id,
                    name = u.FullName,
                    title = roleMap.ContainsKey(u.RoleCode) ? roleMap[u.RoleCode] : u.RoleCode
                })
                .ToList();

            return managers;
        }
        private DepartmentDTO MapToNodeDTO(Department d)
        {
            return new DepartmentDTO
            {
                id = d.Code,
                code = d.Code,
                name = d.Name,
                children = d.Children?
                    .Select(c => MapToNodeDTO(c))
                    .ToList()
            };
        }

        private void CollectChildRoles(int parentId, Dictionary<int, List<EvaluationFlowRole>> roleLookup, HashSet<string> result)
        {
            if (!roleLookup.ContainsKey(parentId)) return;
            foreach (var child in roleLookup[parentId])
            {
                if (!string.IsNullOrEmpty(child.RoleCode))
                {
                    result.Add(child.RoleCode);
                }
                CollectChildRoles(child.Id, roleLookup, result);
            }
        }
    }
}
