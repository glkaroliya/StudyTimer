using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class FocusModeService(StudyDataStore store)
{
    public FocusModePreference GetByStudentId(int studentId)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        return store.FocusModePreferences.SingleOrDefault(x => x.StudentId == studentId)
            ?? new FocusModePreference
            {
                StudentId = studentId,
                Enabled = false,
                DisableNonCriticalNotifications = true
            };
    }

    public FocusModePreference SetPreference(int studentId, bool enabled, bool disableNonCriticalNotifications = true)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        var existing = store.FocusModePreferences.SingleOrDefault(x => x.StudentId == studentId);
        if (existing is null)
        {
            var created = new FocusModePreference
            {
                StudentId = studentId,
                Enabled = enabled,
                DisableNonCriticalNotifications = disableNonCriticalNotifications
            };

            store.FocusModePreferences.Add(created);
            return created;
        }

        existing.Enabled = enabled;
        existing.DisableNonCriticalNotifications = disableNonCriticalNotifications;
        return existing;
    }

    private void EnsureStudentExists(int studentId)
    {
        if (!store.Students.Any(x => x.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }
    }
}
