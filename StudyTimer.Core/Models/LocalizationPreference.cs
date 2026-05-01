namespace StudyTimer.Core.Models;

public sealed class LocalizationPreference
{
    public required int StudentId { get; init; }
    public required SupportedLanguage Language { get; set; }
}
