namespace StudyTimer.Core.Exceptions;

public sealed class UnauthorizedException : StudyTimerException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
