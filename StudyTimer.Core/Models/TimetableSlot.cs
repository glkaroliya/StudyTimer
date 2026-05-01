namespace StudyTimer.Core.Models;

public sealed class TimetableSlot
{
    public required int Id { get; init; }
    public required int StudentId { get; init; }
    public required int SubjectId { get; init; }
    public required DateOnly Date { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required int DurationMinutes { get; init; }
    public required string ActivityDescription { get; init; }
    public bool Completed { get; set; }
    public bool IsRescheduled { get; init; }
    public int? RescheduledToSlotId { get; init; }
    public DateTimeOffset? ReminderSentAtUtc { get; init; }
}
