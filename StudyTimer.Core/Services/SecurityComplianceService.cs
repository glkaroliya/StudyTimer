using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class SecurityComplianceService(StudyDataStore store, IDateTimeProvider dateTimeProvider)
{
    public SecurityComplianceReport RunChecks()
    {
        var now = dateTimeProvider.UtcNow;
        var lockedUsers = store.Users.Count(x => x.LockedUntilUtc.HasValue && x.LockedUntilUtc.Value > now);
        var highRiskUsers = store.Users.Count(x => x.FailedLoginAttempts >= 3);
        var studentUsersWithoutLink = store.Users.Count(x => x.Role == UserRole.Student && (!x.StudentId.HasValue || !store.Students.Any(s => s.Id == x.StudentId.Value)));
        var hasAdmin = store.Users.Any(x => x.Role == UserRole.Admin);

        var findings = new List<string>();
        if (!hasAdmin)
        {
            findings.Add("No admin user account present.");
        }

        if (lockedUsers > 0)
        {
            findings.Add($"{lockedUsers} user(s) currently locked out.");
        }

        if (highRiskUsers > 0)
        {
            findings.Add($"{highRiskUsers} user(s) have repeated failed login attempts.");
        }

        if (studentUsersWithoutLink > 0)
        {
            findings.Add($"{studentUsersWithoutLink} student account(s) are missing a valid student link.");
        }

        var duplicateUsernames = store.Users
            .GroupBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
            .Count(g => g.Count() > 1);
        if (duplicateUsernames > 0)
        {
            findings.Add($"{duplicateUsernames} duplicate username group(s) detected.");
        }

        return new SecurityComplianceReport
        {
            GeneratedAtUtc = now,
            TotalUsers = store.Users.Count,
            LockedUsers = lockedUsers,
            HighRiskUsers = highRiskUsers,
            StudentUsersWithoutLink = studentUsersWithoutLink,
            HasAdminUser = hasAdmin,
            Findings = findings
        };
    }
}
