using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class ProgressAnalyticsService(StudyDataStore store)
{
    public ProgressAnalytics GetWeekly(int studentId, DateOnly weekStartDate, DateOnly asOfDate)
    {
        Guard.Positive(studentId, nameof(studentId));
        return Build(studentId, weekStartDate, weekStartDate.AddDays(6), asOfDate, 1, d => d.DayOfWeek.ToString()[..3]);
    }

    public ProgressAnalytics GetMonthly(int studentId, int year, int month, DateOnly asOfDate)
    {
        Guard.Positive(studentId, nameof(studentId));
        Guard.Range(month, 1, 12, nameof(month));
        Guard.Range(year, 2000, 2100, nameof(year));

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return Build(studentId, start, end, asOfDate, 1, d => d.ToString("MM-dd"));
    }

    private ProgressAnalytics Build(int studentId, DateOnly start, DateOnly end, DateOnly asOfDate, int stepDays, Func<DateOnly, string> label)
    {
        var activeSlots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && !x.IsRescheduled && x.Date >= start && x.Date <= end)
            .ToList();

        var chart = new List<ProgressChartPoint>();
        for (var d = start; d <= end; d = d.AddDays(stepDays))
        {
            var daySlots = activeSlots.Where(x => x.Date == d).ToList();
            chart.Add(new ProgressChartPoint
            {
                Label = label(d),
                CompletedCount = daySlots.Count(x => x.Completed),
                TotalCount = daySlots.Count
            });
        }

        var total = activeSlots.Count;
        var completed = activeSlots.Count(x => x.Completed);
        var missed = activeSlots.Count(x => !x.Completed && x.Date < asOfDate);

        return new ProgressAnalytics
        {
            StudentId = studentId,
            StartDate = start,
            EndDate = end,
            ChartPoints = chart,
            TotalCount = total,
            CompletedCount = completed,
            MissedCount = missed
        };
    }
}
