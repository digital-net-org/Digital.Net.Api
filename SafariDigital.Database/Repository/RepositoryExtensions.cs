using SafariDigital.Core.Validation;
using SafariDigital.Database.Models.Entity;

namespace SafariDigital.Database.Repository;

public static class RepositoryExtensions
{
    public static Result<T?> GetById<T>(this IRepositoryService<T> repository, Guid id)
        where T : EntityWithGuid => repository.GetByPrimaryKey(id);

    public static async Task<Result<T?>> GetByIdAsync<T>(this IRepositoryService<T> repository, Guid id)
        where T : EntityWithGuid => await repository.GetByPrimaryKeyAsync(id);

    public static Result<T?> GetById<T>(this IRepositoryService<T> repository, int id)
        where T : Entity => repository.GetByPrimaryKey(id);

    public static async Task<Result<T?>> GetByIdAsync<T>(this IRepositoryService<T> repository, int id)
        where T : Entity => await repository.GetByPrimaryKeyAsync(id);

    public static Result<T?> GetById<T>(this RepositoryService<T> repository, Guid id)
        where T : EntityWithGuid => repository.GetByPrimaryKey(id);

    public static async Task<Result<T?>> GetByIdAsync<T>(this RepositoryService<T> repository, Guid id)
        where T : EntityWithGuid => await repository.GetByPrimaryKeyAsync(id);

    public static Result<T?> GetById<T>(this RepositoryService<T> repository, int id)
        where T : Entity => repository.GetByPrimaryKey(id);

    public static async Task<Result<T?>> GetByIdAsync<T>(this RepositoryService<T> repository, int id)
        where T : Entity => await repository.GetByPrimaryKeyAsync(id);
}