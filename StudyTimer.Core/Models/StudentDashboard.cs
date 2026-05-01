namespace StudyTimer.Core.Models;

public sealed class StudentDashboard
{
    public required int StudentId { get; init; }
    public required DateOnly Date { get; init; }
    public required IReadOnlyList<TimetableSlot> Slots { get; init; }
    public required int CompletedCount { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyList<ReviewNote> ReviewNotes { get; init; }
}
