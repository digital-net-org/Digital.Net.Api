using System.Linq.Expressions;

namespace SafariDigital.Database.Repository;

public interface IRepository<T>
    where T : class
{
    public void Create(T entity);
    public Task CreateAsync(T entity);
    public void Delete(T entity);
    public void Update(T entity);
    public IQueryable<T> Get(Expression<Func<T, bool>> expression);
    T? GetByPrimaryKey(params object?[]? id);
    Task<T?> GetByPrimaryKeyAsync(params object?[]? id);
    public void Save();
    public Task SaveAsync();
}