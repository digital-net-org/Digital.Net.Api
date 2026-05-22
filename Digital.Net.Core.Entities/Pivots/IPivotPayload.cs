using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Core.Entities.Pivots;

public interface IPivotPayload<TSelf, in TPivot, TChild>
    where TSelf : IPivotPayload<TSelf, TPivot, TChild>
    where TPivot : class
    where TChild : Entity
{
    Guid? Id { get; set; }

    TChild ToChild();

    void ApplyTo(TChild child);

    /// <summary>
    ///     Apply the payload values to the pivot row itself (e.g. for join tables that carry custom columns).
    /// </summary>
    void ApplyToPivot(TPivot pivot)
    {
    }
}