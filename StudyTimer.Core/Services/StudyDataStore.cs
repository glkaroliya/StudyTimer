using StudyTimer.Core.Models;

namespace StudyTimer.Core.Services;

public sealed class StudyDataStore
{
    private int _nextUserId = 1;
    private int _nextStudentId = 1;
    private int _nextSubjectId = 1;
    private int _nextTimetableId = 1;
    private int _nextReviewId = 1;
    private int _nextAuditLogId = 1;

    public List<User> Users { get; } = [];
    public List<Student> Students { get; } = [];
    public List<Subject> Subjects { get; } = [];
    public List<TimetableSlot> TimetableSlots { get; } = [];
    public List<ReviewNote> ReviewNotes { get; } = [];
    public List<StudentThemePreference> ThemePreferences { get; } = [];
    public List<AccessibilityPreference> AccessibilityPreferences { get; } = [];
    public List<FocusModePreference> FocusModePreferences { get; } = [];
    public List<LocalizationPreference> LocalizationPreferences { get; } = [];
    public List<SyncDeviceState> SyncDeviceStates { get; } = [];
    public List<AuditLogEntry> AuditLogs { get; } = [];

    public int NextUserId() => _nextUserId++;
    public int NextStudentId() => _nextStudentId++;
    public int NextSubjectId() => _nextSubjectId++;
    public int NextTimetableId() => _nextTimetableId++;
    public int NextReviewId() => _nextReviewId++;
    public int NextAuditLogId() => _nextAuditLogId++;

    public StudyDataBackup CreateBackup()
    {
        return new StudyDataBackup
        {
            NextUserId = _nextUserId,
            NextStudentId = _nextStudentId,
            NextSubjectId = _nextSubjectId,
            NextTimetableId = _nextTimetableId,
            NextReviewId = _nextReviewId,
            NextAuditLogId = _nextAuditLogId,
            Users = Users.ToList(),
            Students = Students.ToList(),
            Subjects = Subjects.ToList(),
            TimetableSlots = TimetableSlots.ToList(),
            ReviewNotes = ReviewNotes.ToList(),
            ThemePreferences = ThemePreferences.ToList(),
            AccessibilityPreferences = AccessibilityPreferences.ToList(),
            FocusModePreferences = FocusModePreferences.ToList(),
            LocalizationPreferences = LocalizationPreferences.ToList(),
            SyncDeviceStates = SyncDeviceStates.ToList(),
            AuditLogs = AuditLogs.ToList()
        };
    }

    public void RestoreBackup(StudyDataBackup backup)
    {
        _nextUserId = backup.NextUserId;
        _nextStudentId = backup.NextStudentId;
        _nextSubjectId = backup.NextSubjectId;
        _nextTimetableId = backup.NextTimetableId;
        _nextReviewId = backup.NextReviewId;
        _nextAuditLogId = backup.NextAuditLogId;

        Replace(Users, backup.Users);
        Replace(Students, backup.Students);
        Replace(Subjects, backup.Subjects);
        Replace(TimetableSlots, backup.TimetableSlots);
        Replace(ReviewNotes, backup.ReviewNotes);
        Replace(ThemePreferences, backup.ThemePreferences);
        Replace(AccessibilityPreferences, backup.AccessibilityPreferences);
        Replace(FocusModePreferences, backup.FocusModePreferences);
        Replace(LocalizationPreferences, backup.LocalizationPreferences);
        Replace(SyncDeviceStates, backup.SyncDeviceStates);
        Replace(AuditLogs, backup.AuditLogs);
    }

    private static void Replace<T>(List<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }
}
