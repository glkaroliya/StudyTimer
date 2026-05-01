namespace StudyTimer.Core.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
