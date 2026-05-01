namespace StudyTimer.Core.Exceptions;

public sealed class ValidationException : StudyTimerException
{
    public ValidationException(string message) : base(message)
    {
    }
}
