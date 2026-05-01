using StudyTimer.Core.Services;

namespace StudyTimer.Tests;

public class ExportServiceTests
{
    [Fact]
    public void Export_GeneratesPrintableText_AndPdfBytes()
    {
        var store = new StudyDataStore();
        var student = new StudentService(store).Create("Sophia", 5);
        var subject = new SubjectService(store).Create("Geography");
        var timetable = new TimetableService(store);
        var export = new TimetableExportService(store);
        var date = new DateOnly(2026, 5, 1);

        timetable.Create(student.Id, subject.Id, date, new TimeOnly(7, 30), 25, "Map revision");

        var text = export.GeneratePrintableText(student.Id, date);
        var pdf = export.GenerateSimplePdfBytes(student.Id, date);

        Assert.Contains("StudyTimer Timetable", text);
        Assert.Contains("Sophia", text);
        Assert.NotEmpty(pdf);
        Assert.Equal('%', (char)pdf[0]);
    }
}
