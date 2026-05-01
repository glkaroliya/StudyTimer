using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class TimetableService(StudyDataStore store)
{
    public TimetableSlot Create(int studentId, int subjectId, DateOnly date, TimeOnly startTime, int durationMinutes, string activityDescription)
    {
        ValidateDependencies(studentId, subjectId);
        Guard.Range(durationMinutes, 1, 240, nameof(durationMinutes));
        Guard.NotNullOrWhiteSpace(activityDescription, nameof(activityDescription));

        EnsureNoOverlap(studentId, date, startTime, durationMinutes);

        var slot = new TimetableSlot
        {
            Id = store.NextTimetableId(),
            StudentId = studentId,
            SubjectId = subjectId,
            Date = date,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            ActivityDescription = activityDescription.Trim(),
            Completed = false
        };

        store.TimetableSlots.Add(slot);
        return slot;
    }

    public TimetableSlot Update(int id, int subjectId, DateOnly date, TimeOnly startTime, int durationMinutes, string activityDescription)
    {
        var slot = GetById(id);
        ValidateDependencies(slot.StudentId, subjectId);
        Guard.Range(durationMinutes, 1, 240, nameof(durationMinutes));
        Guard.NotNullOrWhiteSpace(activityDescription, nameof(activityDescription));

        EnsureNoOverlap(slot.StudentId, date, startTime, durationMinutes, id);

        var updated = new TimetableSlot
        {
            Id = slot.Id,
            StudentId = slot.StudentId,
            SubjectId = subjectId,
            Date = date,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            ActivityDescription = activityDescription.Trim(),
            Completed = slot.Completed
        };

        var index = store.TimetableSlots.FindIndex(x => x.Id == id);
        store.TimetableSlots[index] = updated;
        return updated;
    }

    public void Delete(int id)
    {
        var slot = GetById(id);
        store.TimetableSlots.Remove(slot);
    }

    public TimetableSlot GetById(int id)
    {
        Guard.Positive(id, nameof(id));
        return store.TimetableSlots.SingleOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Timetable slot {id} was not found.");
    }

    public IReadOnlyList<TimetableSlot> Search(int? studentId = null, DateOnly? date = null, bool? completed = null)
    {
        IEnumerable<TimetableSlot> query = store.TimetableSlots;

        if (studentId.HasValue)
        {
            Guard.Positive(studentId.Value, nameof(studentId));
            query = query.Where(x => x.StudentId == studentId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(x => x.Date == date.Value);
        }

        if (completed.HasValue)
        {
            query = query.Where(x => x.Completed == completed.Value);
        }

        return query.OrderBy(x => x.Date).ThenBy(x => x.StartTime).ToList();
    }

    public TimetableSlot MarkCompleted(int id, bool completed = true)
    {
        var slot = GetById(id);
        var updated = new TimetableSlot
        {
            Id = slot.Id,
            StudentId = slot.StudentId,
            SubjectId = slot.SubjectId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            DurationMinutes = slot.DurationMinutes,
            ActivityDescription = slot.ActivityDescription,
            Completed = completed
        };

        var index = store.TimetableSlots.FindIndex(x => x.Id == id);
        store.TimetableSlots[index] = updated;
        return updated;
    }

    private void ValidateDependencies(int studentId, int subjectId)
    {
        Guard.Positive(studentId, nameof(studentId));
        Guard.Positive(subjectId, nameof(subjectId));

        if (!store.Students.Any(x => x.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }

        if (!store.Subjects.Any(x => x.Id == subjectId))
        {
            throw new NotFoundException($"Subject {subjectId} was not found.");
        }
    }

    private void EnsureNoOverlap(int studentId, DateOnly date, TimeOnly startTime, int durationMinutes, int? currentId = null)
    {
        var newStart = date.ToDateTime(startTime);
        var newEnd = newStart.AddMinutes(durationMinutes);

        var hasOverlap = store.TimetableSlots
            .Where(x => x.StudentId == studentId && x.Date == date && (!currentId.HasValue || x.Id != currentId.Value))
            .Any(existing =>
            {
                var existingStart = existing.Date.ToDateTime(existing.StartTime);
                var existingEnd = existingStart.AddMinutes(existing.DurationMinutes);
                return newStart < existingEnd && existingStart < newEnd;
            });

        if (hasOverlap)
        {
            throw new ValidationException("Timetable slot overlaps with an existing slot for this student.");
        }
    }
}
