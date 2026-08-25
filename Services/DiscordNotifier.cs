using System.Net.Http;
using System.Text;
using System.Text.Json;
using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services
{
    public class DiscordNotifier
    {
        private readonly string _webhookUrl;

        public DiscordNotifier(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public async Task SendReportAsync(RaidReport report)
        {
            if (report.FightType == "HydraPortal")
            {
                await SendHydraPortalEmbeds(report);
            }
            else
            {
                await SendStandardFightEmbed(report);
            }
        }

        // -------------------------------
        // Standard fights: Raid / Attack / Defense
        // -------------------------------
        private async Task SendStandardFightEmbed(RaidReport report)
        {
            var signedLines = report.SignedUp
                .Select(p => $"{p.Name} — {p.Level}")
                .ToList();

            var notSignedLines = report.NotSignedUp
                .Select(p => $"{p.Name} — {p.Level}")
                .ToList();

            var embed = new
            {
                title = GetFightTitle(report),
                color = 0xF0C042,
                fields = new[]
                {
                    new {
                        name = "Signed up",
                        value = signedLines.Count > 0 ? string.Join("\n", signedLines) : "_None_"
                    },
                    new {
                        name = "Not signed up",
                        value = notSignedLines.Count > 0 ? string.Join("\n", notSignedLines) : "_None_"
                    }
                },
                timestamp = DateTime.UtcNow.ToString("o")
            };

            await SendEmbedAsync(embed);
        }

        private string GetFightTitle(RaidReport report)
        {
            return report.FightType switch
            {
                "Raid" => $"⚔ Raid — {report.FightName}",
                "Attack" => $"⚔ Attack on {report.FightName}",
                "Defense" => $"🛡 Defense against {report.FightName}",
                _ => $"⚔ {report.FightType}"
            };
        }

        // -------------------------------
        // Hydra / Guild Portal
        // -------------------------------
        private async Task SendHydraPortalEmbeds(RaidReport report)
        {
            foreach (var category in report.SignedUpByCategory.Keys
                .Union(report.NotSignedUpByCategory.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var signed = report.SignedUpByCategory.ContainsKey(category)
                    ? report.SignedUpByCategory[category]
                    : new List<PlayerEntry>();

                var notSigned = report.NotSignedUpByCategory.ContainsKey(category)
                    ? report.NotSignedUpByCategory[category]
                    : new List<PlayerEntry>();

                var embed = new
                {
                    title = GetHydraPortalTitle(category),
                    color = category.Equals("Hydra", StringComparison.OrdinalIgnoreCase)
                        ? 0x00AAFF
                        : 0xAA00FF,
                    fields = new[]
                    {
                        new {
                            name = "Signed up",
                            value = signed.Count > 0
                                ? string.Join("\n", signed.Select(p => $"{p.Name} — {p.Level}"))
                                : "_None_"
                        },
                        new {
                            name = "Not signed up",
                            value = notSigned.Count > 0
                                ? string.Join("\n", notSigned.Select(p => $"{p.Name} — {p.Level}"))
                                : "_None_"
                        }
                    },
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                await SendEmbedAsync(embed);
            }
        }

        private string GetHydraPortalTitle(string category)
        {
            if (category.Equals("Hydra", StringComparison.OrdinalIgnoreCase))
                return "🐉 Hydra Participation";

            if (category.Equals("Guild portal", StringComparison.OrdinalIgnoreCase))
                return "🌀 Guild Portal Participation";

            return $"⚔ {category}";
        }

        // -------------------------------
        // Send embed to Discord
        // -------------------------------
        private async Task SendEmbedAsync(object embed)
        {
            var payload = new { embeds = new[] { embed } };
            var json = JsonSerializer.Serialize(payload);

            using var client = new HttpClient();
            await client.PostAsync(_webhookUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));
        }

        public async Task SendWeeklyOffenderEmbedsAsync(
            Dictionary<string, Dictionary<string, Dictionary<string, int>>> offendersByType)
        {
            foreach (var fightType in offendersByType.Keys)
            {
                foreach (var category in offendersByType[fightType].Keys)
                {
                    var offenders = offendersByType[fightType][category];

                    if (offenders.Count == 0)
                        continue;

                    var lines = offenders
                        .OrderByDescending(o => o.Value)
                        .Select(o => $"{o.Key} — {o.Value} missed fights")
                        .ToList();

                    var title = GetWeeklyOffenderTitle(fightType, category);

                    var embed = new
                    {
                        title,
                        color = 0xFF0000,
                        fields = new[]
                        {
                    new {
                        name = "Offenders",
                        value = string.Join("\n", lines)
                    }
                },
                        timestamp = DateTime.UtcNow.ToString("o")
                    };

                    await SendEmbedAsync(embed);
                }
            }
        }

        private string GetWeeklyOffenderTitle(string fightType, string category)
        {
            if (fightType.Equals("HydraPortal", StringComparison.OrdinalIgnoreCase))
            {
                if (category.Equals("Hydra", StringComparison.OrdinalIgnoreCase))
                    return "🐉 Weekly Hydra Offenders";

                if (category.Equals("Guild portal", StringComparison.OrdinalIgnoreCase))
                    return "🌀 Weekly Guild Portal Offenders";

                return $"⚔ Weekly {category} Offenders";
            }

            // Standard fights
            return fightType switch
            {
                "Raid" => "⚔ Weekly Raid Offenders",
                "Attack" => "⚔ Weekly Attack Offenders",
                "Defense" => "🛡 Weekly Defense Offenders",
                _ => $"⚔ Weekly {fightType} Offenders"
            };
        }
    }
}
