using System.Text.Json;
using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services;

public class HistoryStore
{
    private readonly string folder = "history";

    public HistoryStore()
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }

    public void Save(RaidReport report)
    {
        var file = Path.Combine(folder, $"{report.Timestamp:yyyy-MM-dd_HH-mm}.json");
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(file, json);
    }

    public IEnumerable<RaidReport> LoadAll()
    {
        foreach (var file in Directory.GetFiles(folder, "*.json"))
        {
            var json = File.ReadAllText(file);
            yield return JsonSerializer.Deserialize<RaidReport>(json);
        }
    }

    public IEnumerable<RaidReport> LoadLast7Days()
    {
        var cutoff = DateTime.Now.AddDays(-7);

        return LoadAll().Where(r => r.Timestamp >= cutoff);
    }
}