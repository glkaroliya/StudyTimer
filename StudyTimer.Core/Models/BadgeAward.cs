namespace StudyTimer.Core.Models;

public sealed class BadgeAward
{
    public required BadgeType Badge { get; init; }
    public required DateOnly AwardedOn { get; init; }
    public required string Reason { get; init; }
}
