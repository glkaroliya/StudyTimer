namespace StudyTimer.Core.Models;

public sealed class SyncDeviceState
{
    public required int StudentId { get; init; }
    public required string DeviceId { get; init; }
    public required string LastSyncToken { get; set; }
    public required DateTimeOffset LastSyncedAtUtc { get; set; }
}
