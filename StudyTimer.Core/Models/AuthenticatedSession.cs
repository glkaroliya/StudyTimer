namespace StudyTimer.Core.Models;

public sealed class AuthenticatedSession
{
    public required User User { get; init; }
    public required string AccessToken { get; init; }
}
