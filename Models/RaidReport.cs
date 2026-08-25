using System;
using System.Collections.Generic;

namespace SFGameGuildReporter.Models
{
    public class RaidReport
    {
        public string FightType { get; set; } = "Unknown";   // Raid, Attack, Defense, HydraPortal
        public string FightName { get; set; } = string.Empty; // Raid name or target guild

        // Legacy flat lists (for simple reports)
        public List<PlayerEntry> SignedUp { get; set; } = new();
        public List<PlayerEntry> NotSignedUp { get; set; } = new();

        // Hydra / Guild portal and future per-category separation
        public Dictionary<string, List<PlayerEntry>> SignedUpByCategory { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<PlayerEntry>> NotSignedUpByCategory { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}