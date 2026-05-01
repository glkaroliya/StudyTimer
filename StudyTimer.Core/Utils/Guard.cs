using StudyTimer.Core.Exceptions;

namespace StudyTimer.Core.Utils;

internal static class Guard
{
    public static void NotNullOrWhiteSpace(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }
    }

    public static void Positive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new ValidationException($"{fieldName} must be greater than zero.");
        }
    }

    public static void Range(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
        {
            throw new ValidationException($"{fieldName} must be between {min} and {max}.");
        }
    }
}
