using System.Collections;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Messages;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Entities.Seeds;

public abstract class Seeder<T>(
    ILogger<Seeder<T>> logger,
    DigitalContext context
) : ISeed
    where T : Entity
{
    public abstract Task ApplySeed();

    /// <summary>
    ///     Seed data into the database. If the data already exists, it will not be seeded.
    /// </summary>
    /// <remarks>
    ///     The data is compared to existing entities and will not be seeded if they already exist,
    ///     based on the following criteria:
    ///     <ul>
    ///         <li>Id is not compared.</li>
    ///         <li>CreatedAt is not compared.</li>
    ///         <li>UpdatedAt is not compared.</li>
    ///         <li>Any boolean values aren't compared.</li>
    ///     </ul>
    /// </remarks>
    /// <param name="data">
    ///     A list of entities to seed into the database.
    /// </param>
    public async Task<Result<List<T>>> SeedAsync(List<T> data)
    {
        var result = new Result<List<T>>([]);
        var skip = 0;

        foreach (var entity in data)
            try
            {
                var properties = typeof(T)
                    .GetProperties()
                    .Where(PropertyExclusionPredicate())
                    .ToList();

                if (context.Set<T>().Where(BuildQuery(properties, entity)).Any())
                {
                    skip++;
                    continue;
                }

                await context.Set<T>().AddAsync(entity);
                await context.SaveChangesAsync();
                result.Value!.Add(entity);
            }
            catch (Exception e)
            {
                result.AddError(e);
            }

        if (skip > 0)
            logger.LogInformation(
                $"Skipped {skip} {nameof(T)} entities because they already exist."
            );
        if (result.HasError)
            logger.LogError($"One or more entities could not be seeded: {result.Errors}");

        return result;
    }

    private static string BuildQuery(List<PropertyInfo> properties, T entity)
    {
        var query = string.Empty;
        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            if (value is null
                || property.PropertyType.IsEntity()
                || property.Name == "Password"
                || (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType)))
                continue;

            value = property.PropertyType.IsEnum ? (int)value : value;

            value = property.PropertyType == typeof(string) ? $"\"{value}\"" : value;

            query += $"{(query == string.Empty ? query : " &&")} {property.Name} == {value}";
        }

        return query;
    }

    private static Func<PropertyInfo, bool> PropertyExclusionPredicate() =>
        property =>
            property.Name is not ("Id" or "CreatedAt" or "UpdatedAt")
            && property.PropertyType != typeof(bool);
}
