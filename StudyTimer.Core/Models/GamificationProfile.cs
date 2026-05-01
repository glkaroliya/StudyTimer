namespace StudyTimer.Core.Models;

public sealed class GamificationProfile
{
    public required int StudentId { get; init; }
    public required int CompletedSessionCount { get; init; }
    public required int CurrentStreakDays { get; init; }
    public required int LongestStreakDays { get; init; }
    public required int TotalPoints { get; init; }
    public required IReadOnlyList<BadgeAward> Badges { get; init; }
}
