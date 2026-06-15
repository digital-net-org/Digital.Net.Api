using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Lib.Entities.Pivots;

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