using StudyTimer.Core.Services;

namespace StudyTimer.Tests;

public class TimerServiceTests
{
    [Fact]
    public void Tick_CompletesCurrentAndMovesToNextSlot()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Ethan", 4);
        var subject = new SubjectService(store).Create("Math");
        var timetableService = new TimetableService(store);
        var timerService = new TimerService(store);
        var date = new DateOnly(2026, 5, 1);

        var first = timetableService.Create(student.Id, subject.Id, date, new TimeOnly(9, 0), 1, "Slot 1");
        var second = timetableService.Create(student.Id, subject.Id, date, new TimeOnly(9, 5), 1, "Slot 2");

        var state = timerService.Start(student.Id, date);
        var next = timerService.Tick(state, 60);

        Assert.Equal(second.Id, next.CurrentSlotId);
        Assert.True(next.ShouldPlayAlert);
    }

    [Fact]
    public void Tick_OnFinalSlot_EndsDay()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Olivia", 4);
        var subject = new SubjectService(store).Create("Reading");
        var timetableService = new TimetableService(store);
        var timerService = new TimerService(store);
        var date = new DateOnly(2026, 5, 1);

        timetableService.Create(student.Id, subject.Id, date, new TimeOnly(11, 0), 1, "Read story");

        var state = timerService.Start(student.Id, date);
        var final = timerService.Tick(state, 60);

        Assert.True(final.IsDayCompleted);
        Assert.True(final.ShouldPlayAlert);
        Assert.Equal(0, final.RemainingSeconds);
    }
}
