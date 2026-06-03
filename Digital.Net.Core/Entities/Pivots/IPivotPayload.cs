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

    void ApplyToPivot(TPivot pivot)
    {
    }
}