using System.Text;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class TimetableExportService(StudyDataStore store)
{
    public string GeneratePrintableText(int studentId, DateOnly date)
    {
        Guard.Positive(studentId, nameof(studentId));

        var student = store.Students.Single(s => s.Id == studentId);
        var slots = store.TimetableSlots
            .Where(x => x.StudentId == studentId && x.Date == date)
            .OrderBy(x => x.StartTime)
            .ToList();

        var subjectLookup = store.Subjects.ToDictionary(x => x.Id, x => x.Name);
        var sb = new StringBuilder();

        sb.AppendLine("StudyTimer Timetable");
        sb.AppendLine($"Student: {student.Name}");
        sb.AppendLine($"Grade: {student.Grade}");
        sb.AppendLine($"Date: {date:yyyy-MM-dd}");
        sb.AppendLine(new string('-', 42));

        foreach (var slot in slots)
        {
            var subject = subjectLookup.GetValueOrDefault(slot.SubjectId, "Unknown");
            sb.AppendLine($"{slot.StartTime:HH:mm} | {slot.DurationMinutes,3} min | {subject} | {slot.ActivityDescription}");
        }

        return sb.ToString();
    }

    public byte[] GenerateSimplePdfBytes(int studentId, DateOnly date)
    {
        var text = GeneratePrintableText(studentId, date).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var content = new StringBuilder();
        content.AppendLine("BT /F1 11 Tf 40 780 Td");
        foreach (var line in lines)
        {
            content.AppendLine($"({line.TrimEnd()}) Tj");
            content.AppendLine("0 -14 Td");
        }

        content.AppendLine("ET");
        var stream = Encoding.ASCII.GetBytes(content.ToString());

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, leaveOpen: true);

        writer.WriteLine("%PDF-1.4");
        writer.WriteLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        writer.WriteLine("2 0 obj << /Type /Pages /Count 1 /Kids [3 0 R] >> endobj");
        writer.WriteLine("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj");
        writer.WriteLine("4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj");
        writer.WriteLine($"5 0 obj << /Length {stream.Length} >> stream");
        writer.Flush();
        ms.Write(stream, 0, stream.Length);
        writer.WriteLine();
        writer.WriteLine("endstream endobj");
        writer.WriteLine("xref");
        writer.WriteLine("0 6");
        writer.WriteLine("0000000000 65535 f ");
        writer.WriteLine("0000000010 00000 n ");
        writer.WriteLine("0000000060 00000 n ");
        writer.WriteLine("0000000117 00000 n ");
        writer.WriteLine("0000000245 00000 n ");
        writer.WriteLine("0000000315 00000 n ");
        writer.WriteLine("trailer << /Size 6 /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine("0");
        writer.WriteLine("%%EOF");
        writer.Flush();

        return ms.ToArray();
    }
}
