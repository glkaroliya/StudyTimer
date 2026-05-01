using System.Text.Json;
using StudyTimer.Core.Models;

namespace StudyTimer.Core.Services;

public sealed class BackupService(StudyDataStore store)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    public string ExportJson()
    {
        return JsonSerializer.Serialize(store.CreateBackup(), SerializerOptions);
    }

    public void RestoreJson(string json)
    {
        var backup = JsonSerializer.Deserialize<StudyDataBackup>(json, SerializerOptions)
            ?? throw new InvalidOperationException("Backup payload is invalid.");

        store.RestoreBackup(backup);
    }
}
