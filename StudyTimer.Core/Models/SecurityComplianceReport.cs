namespace StudyTimer.Core.Models;

public sealed class SecurityComplianceReport
{
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    public required int TotalUsers { get; init; }
    public required int LockedUsers { get; init; }
    public required int HighRiskUsers { get; init; }
    public required int StudentUsersWithoutLink { get; init; }
    public required bool HasAdminUser { get; init; }
    public required IReadOnlyList<string> Findings { get; init; }
    public bool IsCompliant => HasAdminUser && LockedUsers == 0 && HighRiskUsers == 0 && StudentUsersWithoutLink == 0;
}
