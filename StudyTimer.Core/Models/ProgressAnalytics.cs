namespace StudyTimer.Core.Models;

public sealed class ProgressAnalytics
{
    public required int StudentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required IReadOnlyList<ProgressChartPoint> ChartPoints { get; init; }
    public required int CompletedCount { get; init; }
    public required int MissedCount { get; init; }
    public required int TotalCount { get; init; }
    public double CompletionRate => TotalCount == 0 ? 0 : (double)CompletedCount / TotalCount;
}
