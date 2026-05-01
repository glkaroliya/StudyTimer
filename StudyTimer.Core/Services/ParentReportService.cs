using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class ParentReportService(StudyDataStore store, GamificationService gamificationService)
{
    public ParentProgressSummary BuildSummary(int studentId, DateOnly startDate, DateOnly endDate, DateOnly asOfDate)
    {
        Guard.Positive(studentId, nameof(studentId));
        if (endDate < startDate)
        {
            throw new InvalidOperationException("endDate must be greater than or equal to startDate.");
        }

        var student = store.Students.Single(x => x.Id == studentId);
        var slots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && !x.IsRescheduled && x.Date >= startDate && x.Date <= endDate)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        var profile = gamificationService.GetProfile(studentId, asOfDate);
        var weeklyTrend = BuildWeeklyTrend(slots, startDate, endDate);

        return new ParentProgressSummary
        {
            StudentId = studentId,
            StudentName = student.Name,
            Grade = student.Grade,
            StartDate = startDate,
            EndDate = endDate,
            TotalSessions = slots.Count,
            CompletedSessions = slots.Count(x => x.Completed),
            MissedSessions = slots.Count(x => !x.Completed && x.Date < asOfDate),
            CurrentStreakDays = profile.CurrentStreakDays,
            LongestStreakDays = profile.LongestStreakDays,
            TotalPoints = profile.TotalPoints,
            Badges = profile.Badges,
            WeeklyTrend = weeklyTrend
        };
    }

    private static IReadOnlyList<ProgressChartPoint> BuildWeeklyTrend(IReadOnlyList<TimetableSlot> slots, DateOnly startDate, DateOnly endDate)
    {
        var points = new List<ProgressChartPoint>();
        var cursor = startDate;
        var weekIndex = 1;

        while (cursor <= endDate)
        {
            var weekEnd = cursor.AddDays(6);
            if (weekEnd > endDate)
            {
                weekEnd = endDate;
            }

            var weekSlots = slots.Where(x => x.Date >= cursor && x.Date <= weekEnd).ToList();
            points.Add(new ProgressChartPoint
            {
                Label = $"W{weekIndex}",
                CompletedCount = weekSlots.Count(x => x.Completed),
                TotalCount = weekSlots.Count
            });

            cursor = weekEnd.AddDays(1);
            weekIndex++;
        }

        return points;
    }
}
