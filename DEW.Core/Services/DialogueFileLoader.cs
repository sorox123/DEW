using System.Text.Json;
using DEW.Core.Models;

namespace DEW.Core.Services;

public static class DialogueFileLoader
{
    public static List<DialogueEntry> Load(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var  raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? new Dictionary<string, string>();

        var entries = new List<DialogueEntry>();
        foreach (var pair in raw)
        {
            entries.Add(new DialogueEntry { Key = pair.Key, RawText = pair.Value });
        }

        return entries;
    }
}