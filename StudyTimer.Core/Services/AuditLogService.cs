using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class AuditLogService(StudyDataStore store, IDateTimeProvider dateTimeProvider)
{
    public AuditLogEntry RecordAdminAction(int actorUserId, string action, string entityType, string details)
    {
        Guard.Positive(actorUserId, nameof(actorUserId));
        Guard.NotNullOrWhiteSpace(action, nameof(action));
        Guard.NotNullOrWhiteSpace(entityType, nameof(entityType));
        Guard.NotNullOrWhiteSpace(details, nameof(details));

        var log = new AuditLogEntry
        {
            Id = store.NextAuditLogId(),
            ActorUserId = actorUserId,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            Details = details.Trim(),
            CreatedAtUtc = dateTimeProvider.UtcNow
        };

        store.AuditLogs.Add(log);
        return log;
    }

    public IReadOnlyList<AuditLogEntry> Search(int? actorUserId = null, string? entityType = null)
    {
        IEnumerable<AuditLogEntry> query = store.AuditLogs;

        if (actorUserId.HasValue)
        {
            Guard.Positive(actorUserId.Value, nameof(actorUserId));
            query = query.Where(x => x.ActorUserId == actorUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalized = entityType.Trim();
            query = query.Where(x => x.EntityType.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }
}
