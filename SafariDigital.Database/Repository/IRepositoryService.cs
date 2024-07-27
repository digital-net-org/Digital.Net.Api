using System.Linq.Expressions;
using SafariDigital.Core.Validation;
using SafariDigital.Database.Models;

namespace SafariDigital.Database.Repository;

public interface IRepositoryService<T>
    where T : BaseEntity
{
    public void Create(T entity);
    public void Delete(T entity);
    public void Update(T entity);
    public void Save();
    public Task SaveAsync();
    public Result<List<T>> Get(Expression<Func<T, bool>> expression);
    public Task<Result<List<T>>> GetAsync(Expression<Func<T, bool>> expression);
    Result<T?> GetFirstOrDefault(Expression<Func<T, bool>> expression);
    Task<Result<T?>> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression);
    public Result<T?> GetByPrimaryKey(params object?[]? id);
    public Task<Result<T?>> GetByPrimaryKeyAsync(params object?[]? id);
}