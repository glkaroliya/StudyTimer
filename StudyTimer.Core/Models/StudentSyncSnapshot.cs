namespace StudyTimer.Core.Models;

public sealed class StudentSyncSnapshot
{
    public required int StudentId { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required string SyncToken { get; init; }
    public required IReadOnlyList<TimetableSlot> TimetableSlots { get; init; }
    public required IReadOnlyList<ReviewNote> ReviewNotes { get; init; }
    public StudentThemePreference? ThemePreference { get; init; }
    public AccessibilityPreference? AccessibilityPreference { get; init; }
    public FocusModePreference? FocusModePreference { get; init; }
    public LocalizationPreference? LocalizationPreference { get; init; }
}
