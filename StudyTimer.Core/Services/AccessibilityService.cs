using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class AccessibilityService(StudyDataStore store)
{
    public AccessibilityPreference GetByStudentId(int studentId)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        return store.AccessibilityPreferences.SingleOrDefault(x => x.StudentId == studentId)
            ?? new AccessibilityPreference
            {
                StudentId = studentId,
                FontScale = FontScaleOption.Normal,
                UseDyslexiaFriendlyFont = false,
                HighContrastMode = false
            };
    }

    public AccessibilityPreference SetPreference(int studentId, FontScaleOption fontScale, bool useDyslexiaFriendlyFont, bool highContrastMode)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        var existing = store.AccessibilityPreferences.SingleOrDefault(x => x.StudentId == studentId);
        if (existing is null)
        {
            var created = new AccessibilityPreference
            {
                StudentId = studentId,
                FontScale = fontScale,
                UseDyslexiaFriendlyFont = useDyslexiaFriendlyFont,
                HighContrastMode = highContrastMode
            };

            store.AccessibilityPreferences.Add(created);
            return created;
        }

        existing.FontScale = fontScale;
        existing.UseDyslexiaFriendlyFont = useDyslexiaFriendlyFont;
        existing.HighContrastMode = highContrastMode;
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
