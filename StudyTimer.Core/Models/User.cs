namespace StudyTimer.Core.Models;

public sealed class User
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
    public required UserRole Role { get; init; }
    public int? StudentId { get; init; }
}
