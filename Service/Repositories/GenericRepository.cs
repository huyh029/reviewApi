using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using reviewApi.Models;
using System.Linq.Expressions;
namespace reviewApi.Service.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }
        public IEnumerable<T> GetByIds<TId>(IEnumerable<TId> ids)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var key = entityType.FindPrimaryKey();
            var keyProperty = key.Properties.First(); // assume single key

            return _context.Set<T>()
                .Where(e => ids.Contains(EF.Property<TId>(e, keyProperty.Name)))
                .ToList();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }
        public T FindFirst(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().FirstOrDefault(expression);
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public T GetById(object id)
        {
            return _context.Set<T>().Find(id);
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public T GetByIdInclude(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            // Include tất cả navigation properties được truyền vào
            foreach (var include in includes)
                query = query.Include(include);

            // ✅ Lấy thông tin key từ EF Model
            var entityType = _context.Model.FindEntityType(typeof(T));
            var key = entityType.FindPrimaryKey();
            var keyProperty = key.Properties.First(); // giả định chỉ có 1 key (nếu có composite key thì cần xử lý khác)

            // Dùng EF.Property để lọc theo key thật
            return query.FirstOrDefault(e =>
                EF.Property<object>(e, keyProperty.Name).Equals(id));
        }

    }

}
