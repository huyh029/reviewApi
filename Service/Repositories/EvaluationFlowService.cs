using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using reviewApi.DTO;
using reviewApi.Models;
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
            IEnumerable<EvaluationFlow> EvaluationFlows;
            if (search != null) search = search.ToLower();
            if (string.IsNullOrEmpty(search))
                EvaluationFlows = _iUnitOfWork.EvaluationFlow.GetAll();
            else
                EvaluationFlows = _iUnitOfWork.EvaluationFlow
                    .Find(e => e.Code.ToLower().Contains(search)
                            || e.Name.ToLower().Contains(search));

            int startIndex = (page - 1) * limit;
            _iUnitOfWork.EvaluationFlowDepartment.GetAll();
            _iUnitOfWork.Department.GetAll();
            var data = EvaluationFlows
               .Skip(startIndex)
               .Take(limit)
               .Select((e, index) => new EvaluationFlowDTO
               {
                   code = e.Code,
                   name = e.Name,
                   status = e.Status,
                   applicableDepartments = e.EvaluationFlowDepartments!=null?e.EvaluationFlowDepartments.Select(f => f.Department.Name).ToList():null,
               })
               .ToList();
            return new responseDTO<List<EvaluationFlowDTO>>
            {
                data = data,
                pagination = new Pagination
                {
                    currentPage = page,
                    totalItems = EvaluationFlows.Count(),
                    itemsPerPage = limit,
                    totalPages = (int)Math.Ceiling((double)EvaluationFlows.Count() / limit)
                }
            };
        }
        public async Task<EvaluationFlowDetailDTO> getById(string code)
        {
            var e = _iUnitOfWork.EvaluationFlow.GetById(code);
            if (e == null) throw new Exception("Không tìm thấy quy trình đánh giá");
            _iUnitOfWork.EvaluationFlowDepartment.GetAll();
            _iUnitOfWork.EvaluationFlowRole.GetAll();
            _iUnitOfWork.Role.GetAll();

            return new EvaluationFlowDetailDTO
            {
                code = e.Code,
                name = e.Name,
                status = e.Status,
                applicableDepartments = e.EvaluationFlowDepartments != null ? e.EvaluationFlowDepartments.Select(f => f.DepartmentCode).ToList() : null,
                roleHierarchy = e.EvaluationFlowRoles != null ? e.EvaluationFlowRoles
                    .Where(r => r.ParentId == null)
                    .Select(r => MapToNodeDTO(r))
                    .ToList() : null
            };
        }
        private RoleHierarchyNodeDTO MapToNodeDTO(EvaluationFlowRole d)
        {
            return new RoleHierarchyNodeDTO
            {
                id = d.Id,
                role = new RoleHierarchyDataNodeDTO
                {
                    code = d.RoleCode,
                    name = d.Role.Name
                },
                children = d.Child?
                    .Select(c => MapToNodeDTO(c))
                    .ToList()
            };
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
            _iUnitOfWork.EvaluationFlowRole.GetAll();
            _iUnitOfWork.EvaluationFlowDepartment.GetAll();
            if (en.EvaluationFlowRoles != null)
                _iUnitOfWork.EvaluationFlowRole.RemoveRange(en.EvaluationFlowRoles);
            if (en.EvaluationFlowDepartments != null)
                _iUnitOfWork.EvaluationFlowDepartment.RemoveRange(en.EvaluationFlowDepartments);
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
