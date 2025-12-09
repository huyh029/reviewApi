using reviewApi.Models;

namespace reviewApi.Service.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UnitOfWork(
            AppDbContext context, 
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            
            name = GetCurrentUsername();
            role = GetCurrentRole();
            department = GetCurrentUnit();
            User = new GenericRepository<User>(_context);
            Department = new GenericRepository<Department>(_context);
            Role = new GenericRepository<Role>(_context);
            EvaluationFlow = new GenericRepository<EvaluationFlow>(_context);
            EvaluationFlowRole = new GenericRepository<EvaluationFlowRole>(_context);
            EvaluationFlowDepartment = new GenericRepository<EvaluationFlowDepartment>(_context);
            EvaluationObject = new GenericRepository<EvaluationObject>(_context);
            EvaluationObjectRole = new GenericRepository<EvaluationObjectRole>(_context);
            CriteriaSet = new GenericRepository<CriteriaSet>(_context);
            CriteriaSetObject = new GenericRepository<CriteriaSetObject>(_context);
            Criteria = new GenericRepository<Criteria>(_context);
            Classification = new GenericRepository<Classification>(_context);
            CriteriaSetPeriod = new GenericRepository<CriteriaSetPeriod>(_context);
            Evaluation = new GenericRepository<Evaluation>(_context);
            EvaluationChat = new GenericRepository<EvaluationChat>(_context);
            EvaluationChatFile = new GenericRepository<EvaluationChatFile>(_context);
            EvaluationScore = new GenericRepository<EvaluationScore>(_context);
        }
        

        public string? name { get; private set; }
        public string? role { get; private set; }
        public string? department { get; private set; }
        
        public int? userId
        {
            get
            {
                var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
                if (int.TryParse(userIdString, out int userId))
                {
                    return userId;
                }
                return 0;
            }
        }
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        private string GetCurrentUsername()
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return "";
            }
            return username;
        }
        private string GetCurrentRole()
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst("roleId")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return "";
            }
            return username;
        }
        private string GetCurrentUnit()
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst("department")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return "";
            }
            return username;
        }
        public IGenericRepository<User> User { get; private set; }
        public IGenericRepository<Department> Department { get; private set; }
        public IGenericRepository<Role> Role { get; private set; }
        public IGenericRepository<EvaluationFlow> EvaluationFlow { get; private set; }
        public IGenericRepository<EvaluationFlowRole> EvaluationFlowRole { get; private set; }
        public IGenericRepository<EvaluationFlowDepartment> EvaluationFlowDepartment { get; private set; }
        public IGenericRepository<EvaluationObject> EvaluationObject { get; private set; }
        public IGenericRepository<EvaluationObjectRole> EvaluationObjectRole { get; private set; }
        public IGenericRepository<CriteriaSet> CriteriaSet { get; private set; }
        public IGenericRepository<CriteriaSetObject> CriteriaSetObject { get; private set; }
        public IGenericRepository<Criteria> Criteria { get; private set; }
        public IGenericRepository<Classification> Classification { get; private set; }
        public IGenericRepository<CriteriaSetPeriod> CriteriaSetPeriod { get; private set; }
        public IGenericRepository<Evaluation> Evaluation { get; private set; }
        public IGenericRepository<EvaluationScore> EvaluationScore { get; private set; }
        public IGenericRepository<EvaluationChat> EvaluationChat { get; private set; }
        public IGenericRepository<EvaluationChatFile> EvaluationChatFile { get; private set; }

    }
}
