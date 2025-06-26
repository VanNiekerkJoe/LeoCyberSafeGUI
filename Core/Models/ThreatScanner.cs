using System;
using System.Collections.Generic;

namespace LeoCyberSafe.Core.Models
{
    public class ThreatScanner
    {
        public class ThreatReport
        {
            public List<string> Threats { get; set; } = new List<string>();
            public int SeverityScore { get; set; }
        }

        public ThreatReport GenerateReport()
        {
            var report = new ThreatReport
            {
                Threats = new List<string>
                {
                    "Potential phishing email detected",
                    "Outdated software vulnerability found"
                },
                SeverityScore = 75 // 0-100 scale
            };
            return report;
        }
    }
}