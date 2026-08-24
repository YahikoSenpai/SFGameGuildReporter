using SFGameGuildReporter.Services;
using System.Text.Json;
using static System.Console;

namespace SFGameGuildReporter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Load config
            var configPath = Path.Combine("Config", "config.json");
            if (!File.Exists(configPath))
            {
                WriteLine("Missing config.json in Config folder.");
                return;
            }

            var configJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(configJson);

            if (config == null || string.IsNullOrWhiteSpace(config.Webhook))
            {
                WriteLine("Invalid config.json or missing webhook.");
                return;
            }

            string path;

            if (config.AutoPickNewestReport)
            {
                path = Directory.GetFiles(config.ReportsFolder, "*.txt")
                                .OrderByDescending(File.GetCreationTime)
                                .FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(path))
                {
                    WriteLine("No .txt files found in reports folder.");
                    return;
                }

                WriteLine($"Auto-selected newest report: {Path.GetFileName(path)}");
            }
            else
            {
                Write("Enter path to fight report .txt file: ");
                path = ReadLine() ?? string.Empty;
            }

            if (!File.Exists(path))
            {
                WriteLine("File not found.");
                return;
            }

            string input = File.ReadAllText(path);

            var parser = new ReportParser();
            var report = parser.Parse(input);

            var history = new HistoryStore();
            history.Save(report);

            WriteLine($"Parsed raid: {report.RaidName}");
            WriteLine($"Signed up: {report.SignedUp.Count}");
            WriteLine($"Not signed up: {report.NotSignedUp.Count}");

            var notifier = new DiscordNotifier(config.Webhook);
            await notifier.SendReportEmbedAsync(report);
            //await notifier.SendReportAsync(report);

            WriteLine("Report sent to Discord.");

            var weeklyReports = history.LoadLast7Days();

            var analyzer = new WeeklyOffenderAnalyzer();
            var offenders = analyzer.GetWeeklyOffenders(weeklyReports);

            await notifier.SendWeeklyOffenderEmbedAsync(offenders, config.WeeklyOffenderThreshold);
            //await notifier.SendWeeklyOffenderEmbedAsync(offenders, threshold: 3);
            //await notifier.SendWeeklyWarningsAsync(offenders, threshold: 3);

            WriteLine("Weekly offender report sent.");
        }
    }
}