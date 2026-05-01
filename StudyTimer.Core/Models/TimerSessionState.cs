namespace StudyTimer.Core.Models;

public sealed record TimerSessionState
{
    public required int StudentId { get; init; }
    public required DateOnly Date { get; init; }
    public required int CurrentSlotId { get; init; }
    public required int RemainingSeconds { get; init; }
    public required bool IsDayCompleted { get; init; }
    public required bool ShouldPlayAlert { get; init; }
}
