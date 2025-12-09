using Microsoft.EntityFrameworkCore;
using reviewApi.Models;

namespace reviewApi.Service
{
    public interface IUnitOfWork : IDisposable
    {
        public int? userId { get; }
        public string? name { get;  }
        public string? role { get; }
        public string? department { get; }
        IGenericRepository<User> User { get;}
        IGenericRepository<Department> Department { get;}
        IGenericRepository<Role> Role { get;}
        IGenericRepository<EvaluationFlow> EvaluationFlow { get;}
        IGenericRepository<EvaluationFlowRole> EvaluationFlowRole { get;}
        IGenericRepository<EvaluationFlowDepartment> EvaluationFlowDepartment { get;}
        IGenericRepository<EvaluationObject> EvaluationObject { get;}
        IGenericRepository<EvaluationObjectRole> EvaluationObjectRole { get;}
        IGenericRepository<CriteriaSet> CriteriaSet { get;}
        IGenericRepository<CriteriaSetObject> CriteriaSetObject { get;}
        IGenericRepository<Criteria> Criteria { get;}
        IGenericRepository<Classification> Classification { get;}
        IGenericRepository<CriteriaSetPeriod> CriteriaSetPeriod { get;}
        IGenericRepository<Evaluation> Evaluation { get;}
        IGenericRepository<EvaluationScore> EvaluationScore { get; }
        IGenericRepository<EvaluationChat> EvaluationChat { get;}
        IGenericRepository<EvaluationChatFile> EvaluationChatFile { get;}
        int Complete();
    }

}
