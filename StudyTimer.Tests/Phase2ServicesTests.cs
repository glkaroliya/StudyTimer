using StudyTimer.Core.Models;
using StudyTimer.Core.Services;
using StudyTimer.Tests.Helpers;

namespace StudyTimer.Tests;

public class Phase2ServicesTests
{
    [Fact]
    public void ThemeService_DefaultAndUpdate_Works()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Mia", 4);
        var service = new ThemeService(store);

        var defaults = service.GetByStudentId(student.Id);
        var updated = service.SetTheme(student.Id, ThemeMode.Dark, ThemeVariant.Lavender);

        Assert.Equal(ThemeMode.Light, defaults.Mode);
        Assert.Equal(ThemeVariant.SkyBlue, defaults.Variant);
        Assert.Equal(ThemeMode.Dark, updated.Mode);
        Assert.Equal(ThemeVariant.Lavender, updated.Variant);
    }

    [Fact]
    public void ProgressAnalytics_WeeklyAndMonthly_Works()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Noah", 5);
        var subject = new SubjectService(store).Create("Math");
        var timetable = new TimetableService(store);
        var analytics = new ProgressAnalyticsService(store);

        var weekStart = new DateOnly(2026, 5, 4);
        var s1 = timetable.Create(student.Id, subject.Id, weekStart, new TimeOnly(9, 0), 30, "Algebra");
        timetable.MarkCompleted(s1.Id, true);
        timetable.Create(student.Id, subject.Id, weekStart.AddDays(1), new TimeOnly(9, 0), 30, "Geometry");

        var weekly = analytics.GetWeekly(student.Id, weekStart, weekStart.AddDays(2));
        var monthly = analytics.GetMonthly(student.Id, 2026, 5, weekStart.AddDays(20));

        Assert.Equal(2, weekly.TotalCount);
        Assert.Equal(1, weekly.CompletedCount);
        Assert.Equal(1, weekly.MissedCount);
        Assert.Equal(7, weekly.ChartPoints.Count);

        Assert.True(monthly.TotalCount >= 2);
        Assert.True(monthly.MissedCount >= 1);
    }

    [Fact]
    public void Timetable_MissedAndReschedule_Works()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Ava", 3);
        var subject = new SubjectService(store).Create("Science");
        var timetable = new TimetableService(store);

        var originalDate = new DateOnly(2026, 5, 1);
        var original = timetable.Create(student.Id, subject.Id, originalDate, new TimeOnly(10, 0), 40, "Read chapter");

        var missed = timetable.GetMissedSessions(student.Id, new DateOnly(2026, 5, 3));
        var moved = timetable.Reschedule(original.Id, new DateOnly(2026, 5, 4), new TimeOnly(11, 0));
        var oldSlot = timetable.GetById(original.Id);

        Assert.Single(missed);
        Assert.True(oldSlot.IsRescheduled);
        Assert.Equal(moved.Id, oldSlot.RescheduledToSlotId);
        Assert.Empty(timetable.Search(student.Id, originalDate, null));
        Assert.Single(timetable.Search(student.Id, new DateOnly(2026, 5, 4), null));
    }

    [Fact]
    public void ReminderService_ReturnsPendingAndMarksSent()
    {
        var store = new StudyDataStore();
        var dateProvider = new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 8, 50, 0, TimeSpan.Zero));
        var student = new StudentService(store).Create("Ethan", 4);
        var subject = new SubjectService(store).Create("Reading");
        var timetable = new TimetableService(store);
        var reminders = new ReminderService(store, dateProvider);

        var slot = timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(9, 0), 20, "Story");

        var pending = reminders.GetPendingReminders(student.Id, 15);
        reminders.MarkReminderSent(slot.Id);
        var afterMark = reminders.GetPendingReminders(student.Id, 15);

        Assert.Single(pending);
        Assert.Equal(slot.Id, pending[0].SlotId);
        Assert.Empty(afterMark);
    }

    [Fact]
    public void BackupService_ExportAndRestore_Works()
    {
        var store = new StudyDataStore();
        var students = new StudentService(store);
        var themes = new ThemeService(store);
        var backupService = new BackupService(store);

        var student = students.Create("Sophia", 6);
        themes.SetTheme(student.Id, ThemeMode.Dark, ThemeVariant.CandyPink);

        var json = backupService.ExportJson();
        students.Delete(student.Id);

        Assert.Empty(store.Students);

        backupService.RestoreJson(json);

        Assert.Single(store.Students);
        Assert.Single(store.ThemePreferences);
    }

    [Fact]
    public void AuditLogService_RecordAndSearch_Works()
    {
        var store = new StudyDataStore();
        var audit = new AuditLogService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero)));

        audit.RecordAdminAction(1, "StudentUpdated", "Student", "Updated grade");
        audit.RecordAdminAction(2, "SubjectDeleted", "Subject", "Deleted old subject");

        var studentLogs = audit.Search(entityType: "Student");
        var actorLogs = audit.Search(actorUserId: 2);

        Assert.Single(studentLogs);
        Assert.Single(actorLogs);
        Assert.Equal("SubjectDeleted", actorLogs[0].Action);
    }

    [Fact]
    public void Services_WriteAuditEntries_WhenActorProvided()
    {
        var store = new StudyDataStore();
        var students = new StudentService(store);
        var subjects = new SubjectService(store);
        var timetable = new TimetableService(store);

        var student = students.Create("Liam", 4, actorUserId: 10);
        var subject = subjects.Create("English", actorUserId: 10);
        timetable.Create(student.Id, subject.Id, new DateOnly(2026, 5, 1), new TimeOnly(9, 0), 30, "Reading", actorUserId: 10);

        Assert.Equal(3, store.AuditLogs.Count);
        Assert.All(store.AuditLogs, x => Assert.Equal(10, x.ActorUserId));
    }
}
