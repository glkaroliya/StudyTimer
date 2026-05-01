namespace StudyTimer.Core.Models;

public sealed class Student
{
    public required int Id { get; init; }
    public required string Name { get; set; }
    public required int Grade { get; set; }
    public required bool IsActive { get; set; }
}
