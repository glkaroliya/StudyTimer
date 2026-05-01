namespace StudyTimer.Core.Models;

public sealed class StudyDataBackup
{
    public required int NextUserId { get; init; }
    public required int NextStudentId { get; init; }
    public required int NextSubjectId { get; init; }
    public required int NextTimetableId { get; init; }
    public required int NextReviewId { get; init; }
    public required int NextAuditLogId { get; init; }
    public required IReadOnlyList<User> Users { get; init; }
    public required IReadOnlyList<Student> Students { get; init; }
    public required IReadOnlyList<Subject> Subjects { get; init; }
    public required IReadOnlyList<TimetableSlot> TimetableSlots { get; init; }
    public required IReadOnlyList<ReviewNote> ReviewNotes { get; init; }
    public required IReadOnlyList<StudentThemePreference> ThemePreferences { get; init; }
    public required IReadOnlyList<AuditLogEntry> AuditLogs { get; init; }
}
