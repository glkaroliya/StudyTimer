using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class TimerService(StudyDataStore store)
{
    public TimerSessionState Start(int studentId, DateOnly date)
    {
        Guard.Positive(studentId, nameof(studentId));

        var slots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && x.Date == date && !x.IsRescheduled)
            .OrderBy(x => x.StartTime)
            .ToList();

        if (slots.Count == 0)
        {
            throw new NotFoundException("No study slots found for the selected date.");
        }

        var current = slots.FirstOrDefault(x => !x.Completed) ?? slots[0];
        var isFocusModeEnabled = store.FocusModePreferences.SingleOrDefault(x => x.StudentId == studentId)?.Enabled ?? false;
        return new TimerSessionState
        {
            StudentId = studentId,
            Date = date,
            CurrentSlotId = current.Id,
            RemainingSeconds = current.DurationMinutes * 60,
            IsDayCompleted = slots.All(x => x.Completed),
            ShouldPlayAlert = false,
            IsFocusModeEnabled = isFocusModeEnabled
        };
    }

    public TimerSessionState Tick(TimerSessionState currentState, int elapsedSeconds)
    {
        Guard.Positive(elapsedSeconds, nameof(elapsedSeconds));
        if (currentState.IsDayCompleted)
        {
            return currentState with { ShouldPlayAlert = false };
        }

        var currentSlot = store.TimetableSlots.SingleOrDefault(x => x.Id == currentState.CurrentSlotId)
                          ?? throw new NotFoundException($"Current slot {currentState.CurrentSlotId} not found.");

        var remaining = currentState.RemainingSeconds - elapsedSeconds;
        if (remaining > 0)
        {
            return currentState with { RemainingSeconds = remaining, ShouldPlayAlert = false };
        }

        var updatedCurrent = new TimetableSlot
        {
            Id = currentSlot.Id,
            StudentId = currentSlot.StudentId,
            SubjectId = currentSlot.SubjectId,
            Date = currentSlot.Date,
            StartTime = currentSlot.StartTime,
            DurationMinutes = currentSlot.DurationMinutes,
            ActivityDescription = currentSlot.ActivityDescription,
            Completed = true,
            IsRescheduled = currentSlot.IsRescheduled,
            RescheduledToSlotId = currentSlot.RescheduledToSlotId,
            ReminderSentAtUtc = currentSlot.ReminderSentAtUtc
        };

        var currentIndex = store.TimetableSlots.FindIndex(x => x.Id == currentSlot.Id);
        store.TimetableSlots[currentIndex] = updatedCurrent;

        var nextSlot = store.TimetableSlots
            .Where(x => x.StudentId == currentState.StudentId && x.Date == currentState.Date && !x.Completed && !x.IsRescheduled)
            .OrderBy(x => x.StartTime)
            .FirstOrDefault();

        if (nextSlot is null)
        {
            return currentState with
            {
                RemainingSeconds = 0,
                IsDayCompleted = true,
                ShouldPlayAlert = true
            };
        }

        return currentState with
        {
            CurrentSlotId = nextSlot.Id,
            RemainingSeconds = nextSlot.DurationMinutes * 60,
            ShouldPlayAlert = true,
            IsDayCompleted = false
        };
    }
}
