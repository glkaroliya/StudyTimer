namespace StudyTimer.Core.Models;

public sealed class ProgressChartPoint
{
    public required string Label { get; init; }
    public required int CompletedCount { get; init; }
    public required int TotalCount { get; init; }
}
