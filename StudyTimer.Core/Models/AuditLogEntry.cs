namespace StudyTimer.Core.Models;

public sealed class AuditLogEntry
{
    public required int Id { get; init; }
    public int? ActorUserId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string Details { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
