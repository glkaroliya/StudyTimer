using StudyTimer.Core.Abstractions;

namespace StudyTimer.Tests.Helpers;

internal sealed class TestDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}
