using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using reviewApi.DTO;
using reviewApi.Models;
using System.Linq.Expressions;
using System.Linq;

namespace reviewApi.Service.Repositories
{
    public class EvaluationFlowService:IEvaluationFlowService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        public EvaluationFlowService(
            IUnitOfWork iUnitOfWork
         )
        {
            _iUnitOfWork = iUnitOfWork;
        }
        public async Task<responseDTO<List<EvaluationFlowDTO>>> getEvaluationFlows(string? search,int page,int limit)
        {
            if (search != null) search = search.ToLower();
            Expression<Func<EvaluationFlow, bool>> filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = e => e.Code.ToLower().Contains(search) || e.Name.ToLower().Contains(search);
            }

            var totalItems = _iUnitOfWork.EvaluationFlow.Count(filter);
            int startIndex = (page - 1) * limit;

            var flowsPage = _iUnitOfWork.EvaluationFlow.GetPaged(startIndex, limit, filter).ToList();
            var flowCodes = flowsPage.Select(f => f.Code).ToList();

            var flowDepartments = _iUnitOfWork.EvaluationFlowDepartment
                .Find(d => flowCodes.Contains(d.EvaluationFlowCode)).ToList();

            var deptCodes = flowDepartments.Select(d => d.DepartmentCode).Distinct().ToList();
            var departments = _iUnitOfWork.Department.Find(d => deptCodes.Contains(d.Code)).ToList();
            var deptNameMap = departments.ToDictionary(d => d.Code, d => d.Name);

            var data = flowsPage
                .Select(e => new EvaluationFlowDTO
                {
                    code = e.Code,
                    name = e.Name,
                    status = e.Status,
                    applicableDepartments = flowDepartments
                        .Where(fd => fd.EvaluationFlowCode == e.Code)
                        .Select(fd => deptNameMap.ContainsKey(fd.DepartmentCode) ? deptNameMap[fd.DepartmentCode] : fd.DepartmentCode)
                        .ToList()
                })
                .ToList();
            return new responseDTO<List<EvaluationFlowDTO>>
            {
                data = data,
                pagination = new Pagination
                {
                    currentPage = page,
                    totalItems = totalItems,
                    itemsPerPage = limit,
                    totalPages = (int)Math.Ceiling((double)totalItems / limit)
                }
            };
        }
        public async Task<EvaluationFlowDetailDTO> getById(string code)
        {
            var e = _iUnitOfWork.EvaluationFlow.GetById(code);
            if (e == null) throw new Exception("Không tìm thấy quy trình đánh giá");

            var flowDepartments = _iUnitOfWork.EvaluationFlowDepartment
                .Find(d => d.EvaluationFlowCode == code).ToList();
            var deptCodes = flowDepartments.Select(d => d.DepartmentCode).Distinct().ToList();

            var flowRoles = _iUnitOfWork.EvaluationFlowRole
                .Find(r => r.EvaluationFlowCode == code).ToList();
            var roleCodes = flowRoles.Select(r => r.RoleCode).Distinct().ToList();
            var roles = _iUnitOfWork.Role.Find(r => roleCodes.Contains(r.Code)).ToList();
            var roleNameMap = roles.ToDictionary(r => r.Code, r => r.Name);

            var roleTree = BuildRoleTree(flowRoles, roleNameMap);

            return new EvaluationFlowDetailDTO
            {
                code = e.Code,
                name = e.Name,
                status = e.Status,
                applicableDepartments = deptCodes,
                roleHierarchy = roleTree
            };
        }
        private List<RoleHierarchyNodeDTO> BuildRoleTree(List<EvaluationFlowRole> roles, Dictionary<string, string> roleNameMap)
        {
            var nodeMap = roles.ToDictionary(r => r.Id, r => new RoleHierarchyNodeDTO
            {
                id = r.Id,
                role = new RoleHierarchyDataNodeDTO
                {
                    code = r.RoleCode,
                    name = roleNameMap.ContainsKey(r.RoleCode) ? roleNameMap[r.RoleCode] : r.RoleCode
                },
                children = new List<RoleHierarchyNodeDTO>()
            });

            var roots = new List<RoleHierarchyNodeDTO>();
            foreach (var r in roles)
            {
                if (r.ParentId.HasValue && nodeMap.ContainsKey(r.ParentId.Value))
                {
                    nodeMap[r.ParentId.Value].children.Add(nodeMap[r.Id]);
                }
                else
                {
                    roots.Add(nodeMap[r.Id]);
                }
            }
            return roots;
        }
        public async Task creatEvaluationFlow(EvaluationFlowDetailDTO payload)
        {
            if (_iUnitOfWork.EvaluationFlow.GetById(payload.code) != null) throw new Exception("Mã luồng đánh giá này đã tồn tại!");
            var evaluationDepartment = payload.applicableDepartments!=null? payload.applicableDepartments.Select(e => new EvaluationFlowDepartment
            {
                DepartmentCode = e,
                EvaluationFlowCode = payload.code
            }).ToList() : [];
            var role = payload.roleHierarchy.Select(e => MapToModel(e,payload.code)).ToList();
            _iUnitOfWork.EvaluationFlow.Add(new EvaluationFlow
            {
                Code = payload.code,
                Name = payload.name,
                Status = payload.status,
                EvaluationFlowDepartments = evaluationDepartment,
                EvaluationFlowRoles = role
            });
            _iUnitOfWork.Complete();
        }
        public async Task editEvaluationFlow(EvaluationFlowDetailDTO payload)
        {
            var en = _iUnitOfWork.EvaluationFlow.GetById(payload.code);
            if (en == null) throw new Exception("Mã luồng đánh giá này chưa tồn tại!");
            var evaluationDepartment = payload.applicableDepartments != null ? payload.applicableDepartments.Select(e => new EvaluationFlowDepartment
            {
                DepartmentCode = e,
                EvaluationFlowCode = payload.code
            }).ToList() : [];
            var role = payload.roleHierarchy.Select(e => MapToModel(e,payload.code)).ToList();
            var existingRoles = _iUnitOfWork.EvaluationFlowRole.Find(r => r.EvaluationFlowCode == payload.code).ToList();
            var existingDepts = _iUnitOfWork.EvaluationFlowDepartment.Find(d => d.EvaluationFlowCode == payload.code).ToList();
            if (existingRoles.Any())
                _iUnitOfWork.EvaluationFlowRole.RemoveRange(existingRoles);
            if (existingDepts.Any())
                _iUnitOfWork.EvaluationFlowDepartment.RemoveRange(existingDepts);
            en.Name = payload.name;
            en.Status = payload.status;
            en.EvaluationFlowDepartments = evaluationDepartment;
            en.EvaluationFlowRoles = role;
            _iUnitOfWork.EvaluationFlow.Update(en);
            _iUnitOfWork.Complete();
        }
        private EvaluationFlowRole MapToModel(RoleHierarchyNodeDTO d,string evaluationRoleCode)
        {
            return new EvaluationFlowRole
            {
                EvaluationFlowCode = evaluationRoleCode,
                RoleCode = d.role.code,
                Child = d.children?
                    .Select(c => MapToModel(c,evaluationRoleCode))
                    .ToList()
            };
        }
        public async Task deleteEvalutionFlow(string code)
        {
            var e = _iUnitOfWork.EvaluationFlow.GetById(code);
            if (e == null) throw new Exception("Đối tượng đánh giá này chưa tồn tại!");
            _iUnitOfWork.EvaluationFlow.Remove(e);
            _iUnitOfWork.Complete();
        }
        public async Task deleteEvalutionFlows(DeleteMultiple<string> payload)
        {
            if (payload.all == null || payload.all == false)
            {
                if (payload.ids != null)
                    _iUnitOfWork.EvaluationFlow.RemoveRange(_iUnitOfWork.EvaluationFlow.GetByIds(payload.ids));
            }
            else
            {
                var ids1 = _iUnitOfWork.EvaluationFlow.Find(e => (payload.filters.search == null ? true : (e.Name.ToLower().Contains(payload.filters.search)|| e.Code.ToLower().Contains(payload.filters.search)))).Select(e => e.Code).ToList();
                var common = payload.exclude == null ? ids1.ToList() : ids1.Except(payload.exclude).ToList();
                _iUnitOfWork.EvaluationFlow.RemoveRange(_iUnitOfWork.EvaluationFlow.GetByIds(common));
            }
            _iUnitOfWork.Complete();
        }
    }
}
