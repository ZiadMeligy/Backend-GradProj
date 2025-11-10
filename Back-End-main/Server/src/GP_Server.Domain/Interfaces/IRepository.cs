using System.Linq.Expressions;

namespace GP_Server.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<(IEnumerable<T>, int)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null);
}
