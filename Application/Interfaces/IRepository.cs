using System.Linq.Expressions;

namespace Application.Interfaces
{

    public interface IRepository<T> where T : class
    {
        // عملیات اصلی CRUD
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        T Update(T entity);
        Task UpdateAsync(T entity);
        void Delete(T entity);
        Task DeleteAsync(T entity);  // ← اضافه شد (Async)
      
        Task<int> SaveChangesAsync();

        // کوئری‌های پیشرفته
        IQueryable<T> Query();
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // عملیات توده‌ای
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task UpdateRangeAsync(IEnumerable<T> entities);
    }
}
