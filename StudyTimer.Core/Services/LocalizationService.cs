using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class LocalizationService(StudyDataStore store)
{
    public LocalizationPreference GetByStudentId(int studentId)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        return store.LocalizationPreferences.SingleOrDefault(x => x.StudentId == studentId)
            ?? new LocalizationPreference
            {
                StudentId = studentId,
                Language = SupportedLanguage.English
            };
    }

    public LocalizationPreference SetLanguage(int studentId, SupportedLanguage language)
    {
        Guard.Positive(studentId, nameof(studentId));
        EnsureStudentExists(studentId);

        var existing = store.LocalizationPreferences.SingleOrDefault(x => x.StudentId == studentId);
        if (existing is null)
        {
            var created = new LocalizationPreference
            {
                StudentId = studentId,
                Language = language
            };

            store.LocalizationPreferences.Add(created);
            return created;
        }

        existing.Language = language;
        return existing;
    }

    public string Translate(string key, SupportedLanguage language)
    {
        return (key, language) switch
        {
            ("ReminderUpcomingSession", SupportedLanguage.Spanish) => "Sesión próxima a las {0}: {1}",
            ("ReminderUpcomingSession", SupportedLanguage.Hindi) => "अगला अध्ययन सत्र {0} पर: {1}",
            ("ParentProgressSummaryTitle", SupportedLanguage.Spanish) => "Resumen de Progreso para Padres",
            ("ParentProgressSummaryTitle", SupportedLanguage.Hindi) => "अभिभावक प्रगति सारांश",
            _ => key switch
            {
                "ReminderUpcomingSession" => "Upcoming session at {0}: {1}",
                "ParentProgressSummaryTitle" => "Parent Progress Summary",
                _ => key
            }
        };
    }

    private void EnsureStudentExists(int studentId)
    {
        if (!store.Students.Any(x => x.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }
    }
}
