using StudyTimer.Core.Abstractions;

namespace StudyTimer.Core.Utils;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
