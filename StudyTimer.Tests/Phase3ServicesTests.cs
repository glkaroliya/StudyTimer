using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Services;
using StudyTimer.Tests.Helpers;

namespace StudyTimer.Tests;

public class Phase3ServicesTests
{
    [Fact]
    public void AccessibilityLocalizationAndFocusPreferences_WorkWithDefaultsAndUpdates()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Aria", 4);

        var accessibility = new AccessibilityService(store);
        var localization = new LocalizationService(store);
        var focus = new FocusModeService(store);

        var defaultAccessibility = accessibility.GetByStudentId(student.Id);
        var defaultLocalization = localization.GetByStudentId(student.Id);
        var defaultFocus = focus.GetByStudentId(student.Id);

        var updatedAccessibility = accessibility.SetPreference(student.Id, FontScaleOption.ExtraLarge, true, true);
        var updatedLocalization = localization.SetLanguage(student.Id, SupportedLanguage.Spanish);
        var updatedFocus = focus.SetPreference(student.Id, enabled: true, disableNonCriticalNotifications: true);

        Assert.Equal(FontScaleOption.Normal, defaultAccessibility.FontScale);
        Assert.Equal(SupportedLanguage.English, defaultLocalization.Language);
        Assert.False(defaultFocus.Enabled);

        Assert.Equal(FontScaleOption.ExtraLarge, updatedAccessibility.FontScale);
        Assert.True(updatedAccessibility.UseDyslexiaFriendlyFont);
        Assert.True(updatedAccessibility.HighContrastMode);
        Assert.Equal(SupportedLanguage.Spanish, updatedLocalization.Language);
        Assert.True(updatedFocus.Enabled);
    }

    [Fact]
    public void TimerSession_ReflectsFocusModeSetting()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Leo", 5);
        var subject = new SubjectService(store).Create("Math");
        new FocusModeService(store).SetPreference(student.Id, enabled: true);
        new TimetableService(store).Create(student.Id, subject.Id, new DateOnly(2026, 5, 2), new TimeOnly(9, 0), 20, "Practice");

        var state = new TimerService(store).Start(student.Id, new DateOnly(2026, 5, 2));

        Assert.True(state.IsFocusModeEnabled);
    }

    [Fact]
    public void ReminderService_LocalizesMessage_ByStudentLanguage()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Nina", 3);
        var subject = new SubjectService(store).Create("Reading");
        new LocalizationService(store).SetLanguage(student.Id, SupportedLanguage.Spanish);

        new TimetableService(store).Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(9, 0), 30, "Story");
        var reminders = new ReminderService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 8, 55, 0, TimeSpan.Zero)));

        var pending = reminders.GetPendingReminders(student.Id, 10);

        Assert.Single(pending);
        Assert.Contains("Sesión próxima", pending[0].Message);
    }

    [Fact]
    public void Gamification_CalculatesStreakPointsAndBadges()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Noah", 5);
        var subject = new SubjectService(store).Create("Science");
        var timetable = new TimetableService(store);

        for (var i = 0; i < 7; i++)
        {
            var slot = timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1).AddDays(i), new TimeOnly(8, 0), 25, $"Session {i}");
            timetable.MarkCompleted(slot.Id, true);
        }

        var profile = new GamificationService(store).GetProfile(student.Id, new DateOnly(2026, 5, 7));

        Assert.Equal(7, profile.CompletedSessionCount);
        Assert.Equal(7, profile.CurrentStreakDays);
        Assert.Equal(7, profile.LongestStreakDays);
        Assert.True(profile.TotalPoints > 0);
        Assert.Contains(profile.Badges, b => b.Badge == BadgeType.Starter);
        Assert.Contains(profile.Badges, b => b.Badge == BadgeType.Consistency);
        Assert.Contains(profile.Badges, b => b.Badge == BadgeType.PerfectWeek);
    }

    [Fact]
    public void ParentReport_BuildsSummaryAndExportsLocalizedTextAndPdf()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Mia", 4);
        var subject = new SubjectService(store).Create("English");
        var timetable = new TimetableService(store);
        new LocalizationService(store).SetLanguage(student.Id, SupportedLanguage.Hindi);

        var s1 = timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(8, 0), 20, "Read");
        timetable.MarkCompleted(s1.Id, true);
        timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 2), new TimeOnly(8, 0), 20, "Write");

        var parentService = new ParentReportService(store, new GamificationService(store));
        var export = new ParentReportExportService(parentService, new LocalizationService(store));
        var summary = parentService.BuildSummary(student.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), new DateOnly(2026, 5, 3));
        var text = export.GeneratePrintableText(student.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), new DateOnly(2026, 5, 3));
        var pdf = export.GeneratePdfBytes(student.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), new DateOnly(2026, 5, 3));

        Assert.Equal(2, summary.TotalSessions);
        Assert.Equal(1, summary.CompletedSessions);
        Assert.Equal(1, summary.MissedSessions);
        Assert.Contains("अभिभावक प्रगति सारांश", text);
        Assert.NotEmpty(pdf);
    }

    [Fact]
    public void CloudSync_PullPushAndConflictDetection_Work()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Kai", 4);
        var subject = new SubjectService(store).Create("Math");
        var timetable = new TimetableService(store);
        var sync = new CloudSyncService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero)));

        timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(9, 0), 20, "A");

        var snapshot = sync.PullSnapshot(student.Id, "device-a");

        timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(10, 0), 20, "B");

        Assert.Throws<ValidationException>(() => sync.PushSnapshot(snapshot, "device-a", snapshot.SyncToken));

        var latest = sync.PullSnapshot(student.Id, "device-a");
        var modified = new StudentSyncSnapshot
        {
            StudentId = latest.StudentId,
            CreatedAtUtc = latest.CreatedAtUtc,
            SyncToken = latest.SyncToken,
            TimetableSlots = latest.TimetableSlots.Where(x => x.ActivityDescription != "B").ToList(),
            ReviewNotes = latest.ReviewNotes,
            ThemePreference = latest.ThemePreference,
            AccessibilityPreference = latest.AccessibilityPreference,
            FocusModePreference = latest.FocusModePreference,
            LocalizationPreference = latest.LocalizationPreference
        };

        var pushed = sync.PushSnapshot(modified, "device-a", latest.SyncToken);

        Assert.Single(store.SyncDeviceStates);
        Assert.NotEqual(latest.SyncToken, pushed.SyncToken);
    }

    [Fact]
    public void AuthService_LocksUserAfterRepeatedFailures_AndSecurityReportReflectsRisk()
    {
        var store = new StudyDataStore();
        var auth = new AuthService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero)));
        auth.RegisterAdmin("admin", "Password123");

        for (var i = 0; i < 4; i++)
        {
            Assert.Throws<UnauthorizedException>(() => auth.Login("admin", "wrong-pass"));
        }

        Assert.Throws<UnauthorizedException>(() => auth.Login("admin", "wrong-pass"));
        var lockException = Assert.Throws<UnauthorizedException>(() => auth.Login("admin", "Password123"));

        var report = new SecurityComplianceService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 12, 1, 0, TimeSpan.Zero))).RunChecks();

        Assert.Contains("locked", lockException.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, report.LockedUsers);
        Assert.False(report.IsCompliant);
    }

    [Fact]
    public void AuthService_RejectsPasswordContainingUsername()
    {
        var store = new StudyDataStore();
        var auth = new AuthService(store);

        var ex = Assert.Throws<ValidationException>(() => auth.RegisterAdmin("teacher", "Teacher123"));

        Assert.Contains("cannot contain username", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
