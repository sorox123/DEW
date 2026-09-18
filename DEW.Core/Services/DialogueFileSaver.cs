using System.Text.Json;
using DEW.Core.Models;

namespace DEW.Core.Services;

public static class DialogueFileSaver
{
    public static void Save(List<DialogueEntry> entries, string filePath)
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            dict[entry.Key] = entry.RawText;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(dict, options);
        File.WriteAllText(filePath, json);
    }
}