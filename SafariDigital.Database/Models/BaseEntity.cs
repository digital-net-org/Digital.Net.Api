using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafariDigital.Database.Models;

public class BaseEntity
{
    [Column("created_at")] [Required] public DateTime CreatedAt { get; set; }

    [Column("updated_at")] public DateTime? UpdatedAt { get; init; }

    public T GetModel<T>()
    {
        var constructor = typeof(T).GetConstructor([GetType()]);
        if (constructor == null)
            throw new InvalidOperationException($"No suitable constructor found for type {typeof(T).Name}");

        return (T)constructor.Invoke([this]);
    }
}

public static class BaseEntityExtensions
{
    public static List<TModel> GetModel<TModel>(this IEnumerable<BaseEntity> entities) where TModel : class
        => entities.Select(entity => entity.GetModel<TModel>()).ToList();
}