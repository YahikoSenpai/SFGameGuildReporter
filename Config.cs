using System;
using System.Collections.Generic;
using System.Text;

namespace SFGameGuildReporter
{
    public class Config
    {
        public string Webhook { get; set; } = string.Empty;
        public string ReportsFolder { get; set; } = "reports";
        public bool AutoPickNewestReport { get; set; } = false;
        public int WeeklyOffenderThreshold { get; set; } = 3;
    }
}
