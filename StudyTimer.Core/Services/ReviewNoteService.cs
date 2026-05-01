using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class ReviewNoteService(StudyDataStore store, IDateTimeProvider dateTimeProvider)
{
    public ReviewNote Add(int studentId, DateOnly date, string note)
    {
        Guard.Positive(studentId, nameof(studentId));
        Guard.NotNullOrWhiteSpace(note, nameof(note));

        if (!store.Students.Any(s => s.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }

        var review = new ReviewNote
        {
            Id = store.NextReviewId(),
            StudentId = studentId,
            Date = date,
            Note = note.Trim(),
            CreatedAtUtc = dateTimeProvider.UtcNow
        };

        store.ReviewNotes.Add(review);
        return review;
    }

    public IReadOnlyList<ReviewNote> GetByStudentAndDate(int studentId, DateOnly date)
    {
        Guard.Positive(studentId, nameof(studentId));

        return store.ReviewNotes
            .Where(x => x.StudentId == studentId && x.Date == date)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }
}
