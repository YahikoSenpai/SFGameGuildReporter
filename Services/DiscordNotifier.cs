using System.Text;
using System.Text.Json;
using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services;

public class DiscordNotifier
{
    private readonly string webhookUrl;

    public DiscordNotifier(string webhookUrl)
    {
        this.webhookUrl = webhookUrl;
    }

    public async Task SendReportEmbedAsync(RaidReport report)
    {
        var missingList = string.Join("\n", report.NotSignedUp.Select(p => p.Name));
        var signedList = string.Join("\n", report.SignedUp.Select(p => p.Name));

        var embed = new
        {
            title = $"🚩🚩🚩 Raid Report — {report.RaidName} 🚩🚩🚩",
            color = 0xF0C042, // gold-ish
            fields = new[]
            {
            new {
                name = $"❌ Missing ({report.NotSignedUp.Count})",
                value = string.IsNullOrWhiteSpace(missingList) ? "_None_" : missingList,
                inline = false
            },
            new {
                name = $"✅ Signed Up ({report.SignedUp.Count})",
                value = string.IsNullOrWhiteSpace(signedList) ? "_None_" : signedList,
                inline = false
            }
        },
            timestamp = DateTime.UtcNow.ToString("o")
        };

        var payload = new
        {
            embeds = new[] { embed }
        };

        var json = JsonSerializer.Serialize(payload);

        using var client = new HttpClient();
        await client.PostAsync(webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }


    public async Task SendReportAsync(RaidReport report)
    {
        var missing = string.Join(", ", report.NotSignedUp.Select(p => p.Name));
        var signed = string.Join(", ", report.SignedUp.Select(p => p.Name));

        var message = $"**Raid Report: {report.RaidName}**\n" +
                      $"❌ **Missing ({report.NotSignedUp.Count})**: {missing}\n" +
                      $"✔️ **Signed Up ({report.SignedUp.Count})**: {signed}";

        var payload = new { content = message };
        var json = JsonSerializer.Serialize(payload);

        using var client = new HttpClient();
        await client.PostAsync(webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task SendWeeklyOffenderEmbedAsync(Dictionary<string, int> offenders, int threshold)
    {
        var badBoys = offenders
            .Where(o => o.Value >= threshold)
            .OrderByDescending(o => o.Value)
            .ToList();

        if (!badBoys.Any())
            return;

        var lines = string.Join("\n", badBoys.Select(o => $"{o.Key} — {o.Value} misses"));

        var embed = new
        {
            title = "⚠⚠⚠ Weekly Guild Fight Offenders ⚠⚠⚠",
            color = 0xFF0000, // red warning
            description = lines,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        var payload = new
        {
            embeds = new[] { embed }
        };

        var json = JsonSerializer.Serialize(payload);

        using var client = new HttpClient();
        await client.PostAsync(webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task SendWeeklyWarningsAsync(Dictionary<string, int> offenders, int threshold)
    {
        var badBoys = offenders
            .Where(o => o.Value >= threshold)
            .OrderByDescending(o => o.Value)
            .ToList();

        if (!badBoys.Any())
            return;

        var lines = badBoys.Select(o => $"{o.Key} — missed {o.Value} fights");

        var message = "**⚠ Weekly Guild Fight Offenders**\n" +
                      string.Join("\n", lines);

        var payload = new { content = message };
        var json = JsonSerializer.Serialize(payload);

        using var client = new HttpClient();
        await client.PostAsync(webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }
}