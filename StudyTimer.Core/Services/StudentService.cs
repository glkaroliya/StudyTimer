using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class StudentService(StudyDataStore store)
{
    public Student Create(string name, int grade, bool isActive = true, int? actorUserId = null)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.Range(grade, 1, 12, nameof(grade));

        var student = new Student
        {
            Id = store.NextStudentId(),
            Name = name.Trim(),
            Grade = grade,
            IsActive = isActive
        };

        store.Students.Add(student);
        AddAuditLog(actorUserId, "StudentCreated", $"StudentId={student.Id};Name={student.Name}");
        return student;
    }

    public Student Update(int id, string name, int grade, bool isActive, int? actorUserId = null)
    {
        var student = GetById(id);
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.Range(grade, 1, 12, nameof(grade));

        student.Name = name.Trim();
        student.Grade = grade;
        student.IsActive = isActive;

        AddAuditLog(actorUserId, "StudentUpdated", $"StudentId={student.Id};Name={student.Name};Grade={student.Grade};IsActive={student.IsActive}");
        return student;
    }

    public void Delete(int id, int? actorUserId = null)
    {
        var student = GetById(id);
        store.Students.Remove(student);
        store.TimetableSlots.RemoveAll(x => x.StudentId == id);
        store.ReviewNotes.RemoveAll(x => x.StudentId == id);
        store.Users.RemoveAll(x => x.StudentId == id);
        store.ThemePreferences.RemoveAll(x => x.StudentId == id);
        AddAuditLog(actorUserId, "StudentDeleted", $"StudentId={id}");
    }

    public Student GetById(int id)
    {
        Guard.Positive(id, nameof(id));
        return store.Students.SingleOrDefault(s => s.Id == id) ?? throw new NotFoundException($"Student {id} was not found.");
    }

    public IReadOnlyList<Student> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return store.Students.OrderBy(s => s.Name).ToList();
        }

        query = query.Trim();
        return store.Students
            .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || s.Grade.ToString() == query)
            .OrderBy(s => s.Name)
            .ToList();
    }

    private void AddAuditLog(int? actorUserId, string action, string details)
    {
        if (!actorUserId.HasValue)
        {
            return;
        }

        store.AuditLogs.Add(new AuditLogEntry
        {
            Id = store.NextAuditLogId(),
            ActorUserId = actorUserId,
            Action = action,
            EntityType = nameof(Student),
            Details = details,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
