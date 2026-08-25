using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services
{
    public class WeeklyOffenderAnalyzer
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            GetWeeklyOffendersByFightType(IEnumerable<RaidReport> reports)
        {
            // Structure:
            // FightType -> Category -> PlayerName -> MissCount
            var result = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(StringComparer.OrdinalIgnoreCase);

            foreach (var report in reports)
            {
                string fightType = report.FightType ?? "Unknown";

                if (!result.ContainsKey(fightType))
                    result[fightType] = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

                // -----------------------------------------
                // HydraPortal: per-category offenders
                // -----------------------------------------
                if (fightType.Equals("HydraPortal", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var category in report.NotSignedUpByCategory.Keys)
                    {
                        if (!result[fightType].ContainsKey(category))
                            result[fightType][category] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        foreach (var player in report.NotSignedUpByCategory[category])
                        {
                            if (!result[fightType][category].ContainsKey(player.Name))
                                result[fightType][category][player.Name] = 0;

                            result[fightType][category][player.Name]++;
                        }
                    }

                    continue;
                }

                // -----------------------------------------
                // Standard fights: Raid / Attack / Defense
                // -----------------------------------------
                const string generalCategory = "General";

                if (!result[fightType].ContainsKey(generalCategory))
                    result[fightType][generalCategory] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var player in report.NotSignedUp)
                {
                    if (!result[fightType][generalCategory].ContainsKey(player.Name))
                        result[fightType][generalCategory][player.Name] = 0;

                    result[fightType][generalCategory][player.Name]++;
                }
            }

            return result;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            FilterOffenders(Dictionary<string, Dictionary<string, Dictionary<string, int>>> offendersByType, int threshold)
        {
            var filtered = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fightType in offendersByType.Keys)
            {
                foreach (var category in offendersByType[fightType].Keys)
                {
                    var offenders = offendersByType[fightType][category]
                        .Where(o => o.Value >= threshold)
                        .ToDictionary(o => o.Key, o => o.Value, StringComparer.OrdinalIgnoreCase);

                    if (offenders.Count > 0)
                    {
                        if (!filtered.ContainsKey(fightType))
                            filtered[fightType] = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

                        filtered[fightType][category] = offenders;
                    }
                }
            }

            return filtered;
        }
    }
}
