namespace StudyTimer.Core.Models;

public sealed class AccessibilityPreference
{
    public required int StudentId { get; init; }
    public required FontScaleOption FontScale { get; set; }
    public required bool UseDyslexiaFriendlyFont { get; set; }
    public required bool HighContrastMode { get; set; }
}
