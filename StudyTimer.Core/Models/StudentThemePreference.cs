namespace StudyTimer.Core.Models;

public sealed class StudentThemePreference
{
    public required int StudentId { get; init; }
    public required ThemeMode Mode { get; set; }
    public required ThemeVariant Variant { get; set; }
}
