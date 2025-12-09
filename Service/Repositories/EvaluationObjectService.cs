using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO;
using reviewApi.Models;
using System.Runtime.InteropServices.Marshalling;

namespace reviewApi.Service.Repositories
{
    public class EvaluationObjectService : IEvaluationObjectService
    {
        private readonly IUnitOfWork _iUnitOfWork;
        public EvaluationObjectService(
            IUnitOfWork iUnitOfWork
         )
        {
            _iUnitOfWork = iUnitOfWork;
        }
        public async Task<responseDTO<List<EvaluationObjectsDTO>>> getObjects(string? search, int limit, int page)
        {
            IEnumerable<EvaluationObject> EvaluationObjects;
            if(search!=null) search = search.ToLower();
            if (string.IsNullOrEmpty(search))
                EvaluationObjects = _iUnitOfWork.EvaluationObject.GetAll();
            else
                EvaluationObjects = _iUnitOfWork.EvaluationObject
                    .Find(e => e.Code.ToLower().Contains(search)
                            || e.Name.ToLower().Contains(search));

            int startIndex = (page - 1) * limit;

             var data = EvaluationObjects
                .Skip(startIndex)
                .Take(limit)
                .Select((e, index) => new EvaluationObjectsDTO
                {
                    id = startIndex + index + 1,
                    code = e.Code,
                    name = e.Name,
                    status = e.Status
                })
                .ToList();
            return new responseDTO<List<EvaluationObjectsDTO>>
            {
                data = data,
                pagination = new Pagination
                {
                    currentPage = page,
                    totalItems = EvaluationObjects.Count(),
                    itemsPerPage = limit,
                    totalPages = (int)Math.Ceiling((double)EvaluationObjects.Count() / limit)
                }
            };
        }
        public async Task deleteObject(string code)
        {
            var e = _iUnitOfWork.EvaluationObject.GetById(code);
            if (e == null) throw new Exception("Mã đối tượng đánh giá này chưa tồn tại!");
            _iUnitOfWork.EvaluationObject.Remove(e);
            _iUnitOfWork.Complete();
        }
        public async Task postObject(EvaluationObjectsDTO payload)
        {
            if (_iUnitOfWork.EvaluationObject.GetById(payload.code) != null) throw new Exception("Mã đánh giá này đã tồn tại");
            _iUnitOfWork.EvaluationObject.Add(new EvaluationObject
            {
                Code = payload.code,
                Name = payload.name,
                Status = payload.status
            });
            _iUnitOfWork.Complete();
        }
        public async Task putObject(EvaluationObjectsDTO payload)
        {

            var e = _iUnitOfWork.EvaluationObject.GetById(payload.code);
            if (e == null) throw new Exception("Không tìm thấy đối tượng cần chỉnh sửa");
            e.Name = payload.name;
            e.Status = payload.status;
            _iUnitOfWork.EvaluationObject.Update(e);
            _iUnitOfWork.Complete();
        }
        public async Task<List<DepartmentNodeDTO>> getEvaluationObjectTree(string? search)
        {
            _iUnitOfWork.EvaluationObjectRole.GetAll();
            _iUnitOfWork.User.GetAll();
            var all = _iUnitOfWork.Department.GetAll();

            var roots = all.Where(x => x.ParentCode == null).ToList();
            var result = roots.Select(r => MapToNodeDTO(r)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                result = result
                    .Select(r => FilterTree(r, search))
                    .Where(r => r != null)
                    .ToList();
            }

            return result!;
        }

        private DepartmentNodeDTO MapToNodeDTO(Department d)
        {
            return new DepartmentNodeDTO
            {
                id = d.Code,
                code = d.Code,
                name = d.Name,
                children = d.Children?
                    .Select(c => MapToNodeDTO(c))
                    .ToList(),
                individuals = d.Users?
                    .Select(u => new IndividualDTO
                    {
                        id = u.Id,
                        name = u.FullName,
                        code = u.Username,
                        selectedObjectIds = u.EvaluationObjectRoles!=null? u.EvaluationObjectRoles.Select(e => e.EvaluationObjectCode).ToList() : null
                    })
                    .ToList()
            };
        }
        private DepartmentNodeDTO? FilterTree(DepartmentNodeDTO node, string search)
        {
            search = search.ToLower();

            bool matchDept = node.name != null &&
                             node.name.ToLower().Contains(search);

            bool matchIndividuals = node.individuals != null &&
                                     node.individuals.Any(i => i.name != null &&
                                          i.name.ToLower().Contains(search));
            var filteredChildren = node.children?
                .Select(c => FilterTree(c, search))
                .Where(c => c != null)
                .ToList();

            bool hasChildrenMatch = filteredChildren != null && filteredChildren.Count > 0;
            if (matchDept)
            {
                node.children = filteredChildren;
                return node;
            }
            if (matchIndividuals)
            {
                node.individuals = node.individuals
                    .Where(i => i.name.ToLower().Contains(search))
                    .ToList();

                node.children = filteredChildren;
                return node;
            }
            if (hasChildrenMatch)
            {
                node.children = filteredChildren;
                node.individuals = new List<IndividualDTO>();
                return node;
            }
            return null;
        }

        public async Task putRolesInEvaluationObject(List<assigmentDTO> assignments)
        {
            _iUnitOfWork.EvaluationObjectRole.RemoveRange(_iUnitOfWork.EvaluationObjectRole.GetAll());
            var roles = assignments
                .SelectMany(a => a.selectedObjectIds.Select(code => new EvaluationObjectRole
                {
                    UserId = a.individualId,
                    EvaluationObjectCode = code
                }))
                .ToList();
            _iUnitOfWork.EvaluationObjectRole.AddRange(roles);
            _iUnitOfWork.Complete();
        }
        public async Task<List<EvaluationObjectsDTO>> activeList()
        {
            var activeObjects = _iUnitOfWork.EvaluationObject
                .Find(e => !string.IsNullOrEmpty(e.Status) && e.Status.ToLower() == "active")
                .Select(e => new EvaluationObjectsDTO
                {
                    code = e.Code,
                    name = e.Name
                })
                .ToList();
            return activeObjects;
        }
    }
}
