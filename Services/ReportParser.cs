using System.Text.RegularExpressions;
using SFGameGuildReporter.Models;

namespace SFGameGuildReporter.Services
{
    public class ReportParser
    {
        private static readonly Regex StripTags =
            new(@"<.*?>", RegexOptions.Compiled);

        // English: "Name (Level 250)"
        // Hungarian: "Name (250. szint)"
        private static readonly Regex PlayerRegex =
            new(@"(.+?) \((?:Level )?(\d+)(?:\. szint)?\)", RegexOptions.Compiled);

        private static readonly string[] NotSignedHeaders =
        {
            "Members that did not sign up",
            "Tagok, akik nem jelentkeztek"
        };

        private static readonly string[] SignedHeaders =
        {
            "Members that signed up",
            "Tagok, akik jelentkeztek"
        };

        public RaidReport Parse(string input)
        {
            var report = new RaidReport();
            string currentSection = string.Empty;

            var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Detect "not signed up" section
                if (NotSignedHeaders.Any(h => line.Contains(h, StringComparison.OrdinalIgnoreCase)))
                {
                    currentSection = "no";
                    continue;
                }

                // Detect "signed up" section
                if (SignedHeaders.Any(h => line.Contains(h, StringComparison.OrdinalIgnoreCase)))
                {
                    currentSection = "yes";
                    continue;
                }

                // Clean SFGame tags
                string cleaned = StripTags.Replace(line, "").Trim();

                // Parse player entries
                var match = PlayerRegex.Match(cleaned);
                if (match.Success)
                {
                    var entry = new PlayerEntry
                    {
                        Name = match.Groups[1].Value.Trim(),
                        Level = int.Parse(match.Groups[2].Value)
                    };

                    if (currentSection == "yes")
                        report.SignedUp.Add(entry);
                    else if (currentSection == "no")
                        report.NotSignedUp.Add(entry);

                    continue;
                }

                // Detect raid name (first line)
                if (report.RaidName == string.Empty && cleaned.Contains("\""))
                {
                    report.RaidName = cleaned.Trim('"');
                }
            }

            report.Timestamp = DateTime.Now;
            return report;
        }
    }
}