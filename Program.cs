using static System.Console;
using SFGameGuildReporter.Models;
using SFGameGuildReporter.Services;

namespace SFGameGuildReporter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Title = "SFGame Guild Reporter";

            // -----------------------------
            // Load config (explicit JSON read)
            // -----------------------------
            var configJson = File.ReadAllText("Config/config.json");
            var config = System.Text.Json.JsonSerializer.Deserialize<Config>(configJson);

            if (config == null)
            {
                WriteLine("Failed to load config.");
                return;
            }

            WriteLine("SFGame Guild Reporter");
            WriteLine("----------------------");

            // -----------------------------
            // Pick report file
            // -----------------------------
            string path;

            if (config.AutoPickNewestReport)
            {
                var newest = Directory.GetFiles(config.ReportsFolder, "*.txt")
                    .OrderByDescending(File.GetLastWriteTime)
                    .FirstOrDefault();

                if (newest == null)
                {
                    WriteLine("No report files found.");
                    return;
                }

                path = newest;
                WriteLine($"Auto-selected newest report: {path}");
            }
            else
            {
                Write("Enter path to report file: ");
                path = ReadLine() ?? "";

                if (!File.Exists(path))
                {
                    WriteLine("File not found.");
                    return;
                }
            }

            // -----------------------------
            // Parse report
            // -----------------------------
            string input = File.ReadAllText(path);

            var parser = new ReportParser();
            var report = parser.Parse(input);

            // -----------------------------
            // Save to history
            // -----------------------------
            var history = new HistoryStore();
            history.Save(report);

            // -----------------------------
            // Console output
            // -----------------------------
            WriteLine($"Fight type: {report.FightType}");
            WriteLine($"Fight name: {report.FightName}");

            WriteLine($"Signed up (flat): {report.SignedUp.Count}");
            WriteLine($"Not signed up (flat): {report.NotSignedUp.Count}");

            foreach (var cat in report.SignedUpByCategory.Keys)
                WriteLine($"Signed up in {cat}: {report.SignedUpByCategory[cat].Count}");

            foreach (var cat in report.NotSignedUpByCategory.Keys)
                WriteLine($"Not signed up in {cat}: {report.NotSignedUpByCategory[cat].Count}");

            // -----------------------------
            // Send main report to Discord
            // -----------------------------
            var notifier = new DiscordNotifier(config.Webhook);
            await notifier.SendReportAsync(report);

            WriteLine("Report sent to Discord.");

            // -----------------------------
            // Weekly offender analysis
            // -----------------------------
            var weeklyReports = history.LoadLast7Days();

            var analyzer = new WeeklyOffenderAnalyzer();
            var offendersByType = analyzer.GetWeeklyOffendersByFightType(weeklyReports);

            var filtered = analyzer.FilterOffenders(offendersByType, config.WeeklyOffenderThreshold);

            await notifier.SendWeeklyOffenderEmbedsAsync(filtered);

            WriteLine("Weekly offender report sent.");

            // -----------------------------
            // End
            // -----------------------------
            WriteLine("Done.");
        }
    }
}
