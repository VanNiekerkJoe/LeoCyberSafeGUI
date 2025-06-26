using System;
using System.Collections.Generic;
using System.Linq;

namespace LeoCyberSafe.Core.Models
{
    public class ThreatReport
    {
        public DateTime ScanTime { get; } = DateTime.Now;
        public List<ThreatItem> Threats { get; } = new();
        public int ThreatScore { get; private set; }
        public string Summary => $"{ThreatScore}/100 - {(ThreatScore > 70 ? "⚠️ High Risk" : "✅ Moderate Risk")}";

        public void AddThreat(string description, SeverityLevel severity)
        {
            Threats.Add(new ThreatItem(description, severity));
            UpdateScore();
        }

        private void UpdateScore()
        {
            ThreatScore = Threats.Sum(t => (int)t.Severity * 25); // Max 100 points
            ThreatScore = Math.Min(ThreatScore, 100);
        }
    }

    public record ThreatItem(string Description, SeverityLevel Severity);

    public enum SeverityLevel
    {
        Low = 1,    // 25 points
        Medium = 2, // 50 points
        High = 3    // 75 points
    }
}