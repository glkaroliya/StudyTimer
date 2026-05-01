using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class GamificationService(StudyDataStore store)
{
    public GamificationProfile GetProfile(int studentId, DateOnly asOfDate)
    {
        Guard.Positive(studentId, nameof(studentId));

        var activeSlots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && !x.IsRescheduled)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        var completedSlots = activeSlots.Where(x => x.Completed).ToList();
        var completedDays = activeSlots
            .GroupBy(x => x.Date)
            .Where(g => g.Any() && g.All(x => x.Completed))
            .Select(g => g.Key)
            .OrderBy(x => x)
            .ToList();
        var completedDaySet = completedDays.ToHashSet();

        var currentStreak = CalculateCurrentStreak(completedDaySet, asOfDate);
        var longestStreak = CalculateLongestStreak(completedDays);
        var badges = CalculateBadges(completedSlots, completedDays, currentStreak, longestStreak, asOfDate);

        var points = completedSlots.Count * 10;
        points += currentStreak * 5;
        points += badges.Count * 20;

        return new GamificationProfile
        {
            StudentId = studentId,
            CompletedSessionCount = completedSlots.Count,
            CurrentStreakDays = currentStreak,
            LongestStreakDays = longestStreak,
            TotalPoints = points,
            Badges = badges
        };
    }

    private static int CalculateCurrentStreak(HashSet<DateOnly> completedDays, DateOnly asOfDate)
    {
        var streak = 0;
        for (var day = asOfDate; completedDays.Contains(day); day = day.AddDays(-1))
        {
            streak++;
        }

        return streak;
    }

    private static int CalculateLongestStreak(IReadOnlyList<DateOnly> completedDays)
    {
        if (completedDays.Count == 0)
        {
            return 0;
        }

        var longest = 1;
        var current = 1;

        for (var i = 1; i < completedDays.Count; i++)
        {
            if (completedDays[i - 1].AddDays(1) == completedDays[i])
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 1;
            }
        }

        return longest;
    }

    private static IReadOnlyList<BadgeAward> CalculateBadges(IReadOnlyList<TimetableSlot> completedSlots, IReadOnlyList<DateOnly> completedDaysOrdered, int currentStreak, int longestStreak, DateOnly asOfDate)
    {
        var badges = new List<BadgeAward>();

        if (completedSlots.Count > 0)
        {
            badges.Add(new BadgeAward
            {
                Badge = BadgeType.Starter,
                AwardedOn = completedSlots.Min(x => x.Date),
                Reason = "Completed first study session"
            });
        }

        if (currentStreak >= 3)
        {
            badges.Add(new BadgeAward
            {
                Badge = BadgeType.Consistency,
                AwardedOn = asOfDate,
                Reason = "Maintained a 3-day active streak"
            });
        }

        if (completedSlots.Count >= 20)
        {
            badges.Add(new BadgeAward
            {
                Badge = BadgeType.Marathon,
                AwardedOn = completedSlots[19].Date,
                Reason = "Completed 20 sessions"
            });
        }

        if (longestStreak >= 7)
        {
            badges.Add(new BadgeAward
            {
                Badge = BadgeType.PerfectWeek,
                AwardedOn = asOfDate,
                Reason = "Completed studies for 7 consecutive days"
            });
        }

        return badges;
    }
}
