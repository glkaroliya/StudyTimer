using StudyTimer.Core.Models;

namespace StudyTimer.Core.Services;

public sealed class StudyDataStore
{
    private int _nextUserId = 1;
    private int _nextStudentId = 1;
    private int _nextSubjectId = 1;
    private int _nextTimetableId = 1;
    private int _nextReviewId = 1;

    public List<User> Users { get; } = [];
    public List<Student> Students { get; } = [];
    public List<Subject> Subjects { get; } = [];
    public List<TimetableSlot> TimetableSlots { get; } = [];
    public List<ReviewNote> ReviewNotes { get; } = [];

    public int NextUserId() => _nextUserId++;
    public int NextStudentId() => _nextStudentId++;
    public int NextSubjectId() => _nextSubjectId++;
    public int NextTimetableId() => _nextTimetableId++;
    public int NextReviewId() => _nextReviewId++;
}
