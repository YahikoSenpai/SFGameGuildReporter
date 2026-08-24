using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services;

public class WeeklyOffenderAnalyzer
{
    public Dictionary<string, int> GetWeeklyOffenders(IEnumerable<RaidReport> reports)
    {
        var offenders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var report in reports)
        {
            foreach (var player in report.NotSignedUp)
            {
                if (!offenders.ContainsKey(player.Name))
                    offenders[player.Name] = 0;

                offenders[player.Name]++;
            }
        }

        return offenders;
    }
}