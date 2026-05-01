using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class ThemeService(StudyDataStore store)
{
    public StudentThemePreference GetByStudentId(int studentId)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        return store.ThemePreferences.SingleOrDefault(x => x.StudentId == studentId)
            ?? new StudentThemePreference
            {
                StudentId = studentId,
                Mode = ThemeMode.Light,
                Variant = ThemeVariant.SkyBlue
            };
    }

    public StudentThemePreference SetTheme(int studentId, ThemeMode mode, ThemeVariant variant)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        var existing = store.ThemePreferences.SingleOrDefault(x => x.StudentId == studentId);
        if (existing is null)
        {
            var created = new StudentThemePreference
            {
                StudentId = studentId,
                Mode = mode,
                Variant = variant
            };

            store.ThemePreferences.Add(created);
            return created;
        }

        existing.Mode = mode;
        existing.Variant = variant;
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
