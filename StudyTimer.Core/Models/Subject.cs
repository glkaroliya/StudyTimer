namespace StudyTimer.Core.Models;

public sealed class Subject
{
    public required int Id { get; init; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}
