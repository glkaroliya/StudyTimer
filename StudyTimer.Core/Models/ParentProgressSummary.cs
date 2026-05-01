namespace StudyTimer.Core.Models;

public sealed class ParentProgressSummary
{
    public required int StudentId { get; init; }
    public required string StudentName { get; init; }
    public required int Grade { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required int TotalSessions { get; init; }
    public required int CompletedSessions { get; init; }
    public required int MissedSessions { get; init; }
    public required int CurrentStreakDays { get; init; }
    public required int LongestStreakDays { get; init; }
    public required int TotalPoints { get; init; }
    public required IReadOnlyList<BadgeAward> Badges { get; init; }
    public required IReadOnlyList<ProgressChartPoint> WeeklyTrend { get; init; }
    public double CompletionRate => TotalSessions == 0 ? 0 : (double)CompletedSessions / TotalSessions;
}
