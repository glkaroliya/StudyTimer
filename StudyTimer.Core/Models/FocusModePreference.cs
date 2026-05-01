namespace StudyTimer.Core.Models;

public sealed class FocusModePreference
{
    public required int StudentId { get; init; }
    public required bool Enabled { get; set; }
    public required bool DisableNonCriticalNotifications { get; set; }
}
