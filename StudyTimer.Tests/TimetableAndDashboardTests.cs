using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Services;
using StudyTimer.Tests.Helpers;

namespace StudyTimer.Tests;

public class TimetableAndDashboardTests
{
    [Fact]
    public void Timetable_CreateSearchMarkCompleted_Works()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Liam", 3);
        var subject = new SubjectService(store).Create("Science");
        var service = new TimetableService(store);
        var date = new DateOnly(2026, 5, 1);

        var slot = service.Create(student.Id, subject.Id, date, new TimeOnly(9, 0), 30, "Read chapter 1");
        service.MarkCompleted(slot.Id, true);

        var results = service.Search(student.Id, date, true);

        Assert.Single(results);
        Assert.True(results[0].Completed);
    }

    [Fact]
    public void Timetable_Overlap_ThrowsValidationException()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Liam", 3);
        var subject = new SubjectService(store).Create("Math");
        var service = new TimetableService(store);
        var date = new DateOnly(2026, 5, 1);

        service.Create(student.Id, subject.Id, date, new TimeOnly(10, 0), 45, "Lesson 1");

        Assert.Throws<ValidationException>(() =>
            service.Create(student.Id, subject.Id, date, new TimeOnly(10, 30), 30, "Lesson 2"));
    }

    [Fact]
    public void Dashboard_IncludesSlotsAndReviewNotes()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Ava", 2);
        var subject = new SubjectService(store).Create("English");
        var timetableService = new TimetableService(store);
        var reviewService = new ReviewNoteService(store, new TestDateTimeProvider(new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero)));
        var dashboardService = new StudentDashboardService(store);
        var date = new DateOnly(2026, 5, 1);

        var slot = timetableService.Create(student.Id, subject.Id, date, new TimeOnly(8, 0), 20, "Vocabulary");
        timetableService.MarkCompleted(slot.Id, true);
        reviewService.Add(student.Id, date, "Great focus today.");

        var dashboard = dashboardService.Get(student.Id, date);

        Assert.Equal(1, dashboard.TotalCount);
        Assert.Equal(1, dashboard.CompletedCount);
        Assert.Equal(0, dashboard.MissedCount);
        Assert.Single(dashboard.ReviewNotes);
    }
}
