namespace StudyTimer.Core.Models;

public sealed class ReminderNotification
{
    public required int SlotId { get; init; }
    public required int StudentId { get; init; }
    public required DateTimeOffset ScheduledSlotStartUtc { get; init; }
    public required DateTimeOffset ReminderTimeUtc { get; init; }
    public required string Message { get; init; }
}
