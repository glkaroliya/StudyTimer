using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class CloudSyncService(StudyDataStore store, IDateTimeProvider dateTimeProvider)
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public StudentSyncSnapshot PullSnapshot(int studentId, string deviceId)
    {
        Guard.Positive(studentId, nameof(studentId));
        Guard.NotNullOrWhiteSpace(deviceId, nameof(deviceId));
        EnsureStudentExists(studentId);

        var snapshot = BuildSnapshot(studentId);
        UpsertDeviceState(studentId, deviceId.Trim(), snapshot.SyncToken);
        return snapshot;
    }

    public StudentSyncSnapshot PushSnapshot(StudentSyncSnapshot snapshot, string deviceId, string baseSyncToken)
    {
        Guard.NotNullOrWhiteSpace(deviceId, nameof(deviceId));
        Guard.NotNullOrWhiteSpace(baseSyncToken, nameof(baseSyncToken));
        Guard.Positive(snapshot.StudentId, nameof(snapshot.StudentId));

        var currentToken = BuildSnapshot(snapshot.StudentId).SyncToken;
        if (!string.Equals(currentToken, baseSyncToken.Trim(), StringComparison.Ordinal))
        {
            throw new ValidationException("Sync conflict detected. Pull latest snapshot and retry.");
        }

        store.TimetableSlots.RemoveAll(x => x.StudentId == snapshot.StudentId);
        store.TimetableSlots.AddRange(snapshot.TimetableSlots);

        store.ReviewNotes.RemoveAll(x => x.StudentId == snapshot.StudentId);
        store.ReviewNotes.AddRange(snapshot.ReviewNotes);

        ReplaceTheme(snapshot.StudentId, snapshot.ThemePreference);
        ReplaceAccessibility(snapshot.StudentId, snapshot.AccessibilityPreference);
        ReplaceFocusMode(snapshot.StudentId, snapshot.FocusModePreference);
        ReplaceLocalization(snapshot.StudentId, snapshot.LocalizationPreference);

        var updated = BuildSnapshot(snapshot.StudentId);
        UpsertDeviceState(snapshot.StudentId, deviceId.Trim(), updated.SyncToken);
        return updated;
    }

    private StudentSyncSnapshot BuildSnapshot(int studentId)
    {
        var timetable = store.TimetableSlots.Where(x => x.StudentId == studentId).OrderBy(x => x.Date).ThenBy(x => x.StartTime).ToList();
        var reviews = store.ReviewNotes.Where(x => x.StudentId == studentId).OrderBy(x => x.Date).ThenBy(x => x.Id).ToList();
        var theme = store.ThemePreferences.SingleOrDefault(x => x.StudentId == studentId);
        var accessibility = store.AccessibilityPreferences.SingleOrDefault(x => x.StudentId == studentId);
        var focus = store.FocusModePreferences.SingleOrDefault(x => x.StudentId == studentId);
        var localization = store.LocalizationPreferences.SingleOrDefault(x => x.StudentId == studentId);

        var payload = new
        {
            StudentId = studentId,
            Timetable = timetable,
            Reviews = reviews,
            Theme = theme,
            Accessibility = accessibility,
            Focus = focus,
            Localization = localization
        };

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        var token = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));

        return new StudentSyncSnapshot
        {
            StudentId = studentId,
            CreatedAtUtc = dateTimeProvider.UtcNow,
            SyncToken = token,
            TimetableSlots = timetable,
            ReviewNotes = reviews,
            ThemePreference = theme,
            AccessibilityPreference = accessibility,
            FocusModePreference = focus,
            LocalizationPreference = localization
        };
    }

    private void EnsureStudentExists(int studentId)
    {
        if (!store.Students.Any(x => x.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }
    }

    private void UpsertDeviceState(int studentId, string deviceId, string token)
    {
        var existing = store.SyncDeviceStates.SingleOrDefault(x => x.StudentId == studentId && x.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            store.SyncDeviceStates.Add(new SyncDeviceState
            {
                StudentId = studentId,
                DeviceId = deviceId,
                LastSyncToken = token,
                LastSyncedAtUtc = dateTimeProvider.UtcNow
            });

            return;
        }

        existing.LastSyncToken = token;
        existing.LastSyncedAtUtc = dateTimeProvider.UtcNow;
    }

    private void ReplaceTheme(int studentId, StudentThemePreference? value)
    {
        store.ThemePreferences.RemoveAll(x => x.StudentId == studentId);
        if (value is not null)
        {
            store.ThemePreferences.Add(value);
        }
    }

    private void ReplaceAccessibility(int studentId, AccessibilityPreference? value)
    {
        store.AccessibilityPreferences.RemoveAll(x => x.StudentId == studentId);
        if (value is not null)
        {
            store.AccessibilityPreferences.Add(value);
        }
    }

    private void ReplaceFocusMode(int studentId, FocusModePreference? value)
    {
        store.FocusModePreferences.RemoveAll(x => x.StudentId == studentId);
        if (value is not null)
        {
            store.FocusModePreferences.Add(value);
        }
    }

    private void ReplaceLocalization(int studentId, LocalizationPreference? value)
    {
        store.LocalizationPreferences.RemoveAll(x => x.StudentId == studentId);
        if (value is not null)
        {
            store.LocalizationPreferences.Add(value);
        }
    }
}
