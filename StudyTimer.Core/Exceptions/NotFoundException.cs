namespace StudyTimer.Core.Exceptions;

public sealed class NotFoundException : StudyTimerException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
