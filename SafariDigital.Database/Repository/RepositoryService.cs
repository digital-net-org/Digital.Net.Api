using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Core.Validation;
using SafariDigital.Database.Models;

namespace SafariDigital.Database.Repository;

public class RepositoryService<T>(IRepository<T> repository) : IRepositoryService<T>
    where T : BaseEntity
{
    public void Create(T entity) => repository.Create(entity);

    public void Update(T entity) => repository.Update(entity);

    public void Delete(T entity) => repository.Delete(entity);

    public Result<List<T>> Get(Expression<Func<T, bool>> expression) =>
        new([.. repository.Get(expression)]);

    public async Task<Result<List<T>>> GetAsync(Expression<Func<T, bool>> expression) =>
        new(await repository.Get(expression).ToListAsync());

    public Result<T?> GetFirstOrDefault(Expression<Func<T, bool>> expression)
    {
        var entity = repository.Get(expression).FirstOrDefault();
        return new Result<T?>(entity);
    }

    public async Task<Result<T?>> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
    {
        var entity = await repository.Get(expression).FirstOrDefaultAsync();
        return new Result<T?>(entity);
    }

    public Result<T?> GetByPrimaryKey(params object?[]? id) =>
        RepositoryUtils.TryQuery(repository.GetByPrimaryKey(id));

    public async Task<Result<T?>> GetByPrimaryKeyAsync(params object?[]? id) =>
        await RepositoryUtils.TryQueryAsync(repository.GetByPrimaryKeyAsync(id));

    public void Save() => repository.Save();

    public async Task SaveAsync() => await repository.SaveAsync();
}