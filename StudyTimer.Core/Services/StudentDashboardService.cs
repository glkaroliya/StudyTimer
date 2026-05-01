using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class StudentDashboardService(StudyDataStore store)
{
    public StudentDashboard Get(int studentId, DateOnly date, DateOnly? asOfDate = null)
    {
        Guard.Positive(studentId, nameof(studentId));
        var currentDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var slots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && x.Date == date && !x.IsRescheduled)
            .OrderBy(x => x.StartTime)
            .ToList();

        var reviews = store.ReviewNotes
            .Where(x => x.StudentId == studentId && x.Date == date)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        return new StudentDashboard
        {
            StudentId = studentId,
            Date = date,
            Slots = slots,
            CompletedCount = slots.Count(x => x.Completed),
            MissedCount = slots.Count(x => !x.Completed && x.Date < currentDate),
            TotalCount = slots.Count,
            ReviewNotes = reviews
        };
    }
}
