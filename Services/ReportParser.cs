using System;
using System.Collections.Generic;
using System.Linq;
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

        // Hydra / Guild portal sub-sections (Hungarian + English, normalized names)
        private static readonly string[] SubSectionHeaders =
        {
            "Hidra",        // Hungarian
            "Hydra",        // English
            "Céhes portál", // Hungarian
            "Guild portal"  // English
        };

        // Top-level fight type patterns (English + Hungarian)
        private static readonly Dictionary<string, string[]> FightTypePatterns =
            new(StringComparer.OrdinalIgnoreCase)
        {
            { "Raid", new[] { "Raid \"", "Portya \"" } },
            { "Attack", new[] { "Attack on", "Támadás " } },
            { "Defense", new[] { "Defense against attacker", "Védekezés " } }
        };

        public RaidReport Parse(string input)
        {
            var report = new RaidReport();

            string currentMainSection = "";   // "yes" or "no"
            string currentSubSection = "";    // "Hydra", "Guild portal"

            var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                             .Select(l => l.Trim())
                             .Where(l => !string.IsNullOrWhiteSpace(l))
                             .ToList();

            // Detect fight type from first line
            if (lines.Count > 0)
            {
                DetectFightTypeAndName(lines[0], report);
            }

            foreach (var rawLine in lines)
            {
                string cleaned = StripTags.Replace(rawLine, "").Trim();

                // Detect main sections
                if (NotSignedHeaders.Any(h => cleaned.Contains(h, StringComparison.OrdinalIgnoreCase)))
                {
                    currentMainSection = "no";
                    currentSubSection = "";
                    continue;
                }

                if (SignedHeaders.Any(h => cleaned.Contains(h, StringComparison.OrdinalIgnoreCase)))
                {
                    currentMainSection = "yes";
                    currentSubSection = "";
                    continue;
                }

                // Detect Hydra / Guild portal sub-sections
                foreach (var sub in SubSectionHeaders)
                {
                    // cleaned line is e.g. "Hidra:" or "Hydra:" or "Céhes portál:" → normalize by removing colon
                    string normalizedClean = cleaned.Replace(":", "").Trim();

                    if (normalizedClean.Equals(sub, StringComparison.OrdinalIgnoreCase))
                    {
                        currentSubSection = sub;

                        if (report.FightType == "Unknown")
                            report.FightType = "HydraPortal";

                        EnsureCategoryExists(report, currentMainSection, currentSubSection);
                        goto NextLine;
                    }
                }

                // Parse player entries
                var match = PlayerRegex.Match(cleaned);
                if (match.Success)
                {
                    var entry = new PlayerEntry
                    {
                        Name = match.Groups[1].Value.Trim(),
                        Level = int.Parse(match.Groups[2].Value)
                    };

                    if (!string.IsNullOrEmpty(currentSubSection))
                    {
                        AddToCategory(report, currentMainSection, currentSubSection, entry);
                    }
                    else
                    {
                        if (currentMainSection == "yes")
                            report.SignedUp.Add(entry);
                        else if (currentMainSection == "no")
                            report.NotSignedUp.Add(entry);
                    }

                    continue;
                }

                // Detect fight name if not already set
                if (string.IsNullOrEmpty(report.FightName) && cleaned.Contains("\""))
                {
                    report.FightName = cleaned.Trim('"');
                }

            NextLine:
                continue;
            }

            report.Timestamp = DateTime.Now;
            return report;
        }

        private static void DetectFightTypeAndName(string firstLine, RaidReport report)
        {
            string cleaned = StripTags.Replace(firstLine, "").Trim();

            foreach (var kvp in FightTypePatterns)
            {
                var type = kvp.Key;
                var patterns = kvp.Value;

                if (patterns.Any(p => cleaned.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    report.FightType = type;

                    if (type == "Raid")
                    {
                        int q1 = cleaned.IndexOf('"');
                        int q2 = cleaned.LastIndexOf('"');
                        if (q1 >= 0 && q2 > q1)
                            report.FightName = cleaned.Substring(q1 + 1, q2 - q1 - 1).Trim();
                    }
                    else
                    {
                        report.FightName = cleaned;
                    }

                    return;
                }
            }
        }

        private static void EnsureCategoryExists(RaidReport report, string mainSection, string subSection)
        {
            if (mainSection == "no")
            {
                if (!report.NotSignedUpByCategory.ContainsKey(subSection))
                    report.NotSignedUpByCategory[subSection] = new List<PlayerEntry>();
            }
            else if (mainSection == "yes")
            {
                if (!report.SignedUpByCategory.ContainsKey(subSection))
                    report.SignedUpByCategory[subSection] = new List<PlayerEntry>();
            }
        }

        private static void AddToCategory(RaidReport report, string mainSection, string subSection, PlayerEntry entry)
        {
            if (mainSection == "no")
            {
                report.NotSignedUpByCategory[subSection].Add(entry);
            }
            else if (mainSection == "yes")
            {
                report.SignedUpByCategory[subSection].Add(entry);
            }
        }
    }
}
