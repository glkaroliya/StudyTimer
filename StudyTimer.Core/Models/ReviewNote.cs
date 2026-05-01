namespace StudyTimer.Core.Models;

public sealed class ReviewNote
{
    public required int Id { get; init; }
    public required int StudentId { get; init; }
    public required DateOnly Date { get; init; }
    public required string Note { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
