using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class ReminderService(StudyDataStore store, IDateTimeProvider dateTimeProvider)
{
    public IReadOnlyList<ReminderNotification> GetPendingReminders(int studentId, int minutesBeforeStart = 10)
    {
        Guard.Positive(studentId, nameof(studentId));
        Guard.Range(minutesBeforeStart, 1, 120, nameof(minutesBeforeStart));

        var now = dateTimeProvider.UtcNow;
        var from = now;
        var to = now.AddMinutes(minutesBeforeStart);

        return store.TimetableSlots
            .Where(x => x.StudentId == studentId && !x.Completed && !x.IsRescheduled && x.ReminderSentAtUtc is null)
            .Select(slot => new { Slot = slot, StartUtc = slot.Date.ToDateTime(slot.StartTime, DateTimeKind.Utc) })
            .Where(x => x.StartUtc >= from.UtcDateTime && x.StartUtc <= to.UtcDateTime)
            .OrderBy(x => x.StartUtc)
            .Select(x => new ReminderNotification
            {
                SlotId = x.Slot.Id,
                StudentId = x.Slot.StudentId,
                ScheduledSlotStartUtc = new DateTimeOffset(x.StartUtc, TimeSpan.Zero),
                ReminderTimeUtc = now,
                Message = $"Upcoming session at {x.Slot.StartTime:HH\\:mm}: {x.Slot.ActivityDescription}"
            })
            .ToList();
    }

    public void MarkReminderSent(int slotId)
    {
        Guard.Positive(slotId, nameof(slotId));
        var slot = store.TimetableSlots.Single(x => x.Id == slotId);
        var index = store.TimetableSlots.FindIndex(x => x.Id == slotId);
        store.TimetableSlots[index] = new TimetableSlot
        {
            Id = slot.Id,
            StudentId = slot.StudentId,
            SubjectId = slot.SubjectId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            DurationMinutes = slot.DurationMinutes,
            ActivityDescription = slot.ActivityDescription,
            Completed = slot.Completed,
            IsRescheduled = slot.IsRescheduled,
            RescheduledToSlotId = slot.RescheduledToSlotId,
            ReminderSentAtUtc = dateTimeProvider.UtcNow
        };
    }
}
