namespace Digital.Net.Api.Core.Interval;

/// <summary>
///     Represents a date interval.
/// </summary>
public class DateRange
{
    public DateTime From { get; set; } = DateTime.MinValue;
    public DateTime To { get; set; } = DateTime.MaxValue;
}