using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class SubjectService(StudyDataStore store)
{
    public Subject Create(string name, string? description = null)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        EnsureUniqueName(name);

        var subject = new Subject
        {
            Id = store.NextSubjectId(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty
        };

        store.Subjects.Add(subject);
        return subject;
    }

    public Subject Update(int id, string name, string? description = null)
    {
        var subject = GetById(id);
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        EnsureUniqueName(name, id);

        subject.Name = name.Trim();
        subject.Description = description?.Trim() ?? string.Empty;
        return subject;
    }

    public void Delete(int id)
    {
        var subject = GetById(id);
        store.Subjects.Remove(subject);
        store.TimetableSlots.RemoveAll(x => x.SubjectId == id);
    }

    public Subject GetById(int id)
    {
        Guard.Positive(id, nameof(id));
        return store.Subjects.SingleOrDefault(s => s.Id == id) ?? throw new NotFoundException($"Subject {id} was not found.");
    }

    public IReadOnlyList<Subject> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return store.Subjects.OrderBy(s => s.Name).ToList();
        }

        query = query.Trim();
        return store.Subjects
            .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || s.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Name)
            .ToList();
    }

    private void EnsureUniqueName(string name, int? existingId = null)
    {
        var normalized = name.Trim();
        var duplicate = store.Subjects.Any(s =>
            s.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!existingId.HasValue || s.Id != existingId.Value));

        if (duplicate)
        {
            throw new ValidationException($"Subject '{normalized}' already exists.");
        }
    }
}
