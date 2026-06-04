namespace Digital.Net.Core.Accessors;

/// <summary>
///     Expose the request accessor origin <see cref="RequestOrigin" />.
/// </summary>
public interface IOriginAccessor
{
    RequestOrigin GetOrigin();
}