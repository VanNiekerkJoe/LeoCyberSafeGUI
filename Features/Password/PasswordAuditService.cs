using System;

namespace LeoCyberSafe.Features.Password
{
    public class PasswordAuditService
    {
        public class AuditResult
        {
            public int score { get; set; }
            public string feedback { get; set; }
        }

        public AuditResult Analyze(string password)
        {
            int score = 0;
            string feedback = "Password analysis:\n";

            if (string.IsNullOrEmpty(password))
            {
                return new AuditResult { score = 0, feedback = "Password cannot be empty." };
            }

            // Basic scoring criteria
            if (password.Length >= 12) score += 30;
            if (password.Any(char.IsUpper)) score += 20;
            if (password.Any(char.IsDigit)) score += 20;
            if (password.Any(ch => "!@#$%^&*".Contains(ch))) score += 20;
            if (password.Contains("password") || password.Contains("1234")) score -= 20;

            score = Math.Max(0, Math.Min(100, score)); // Clamp between 0-100

            feedback += $"Score: {score}/100\n";
            feedback += score >= 70 ? "Strong password!" : "Consider adding length, numbers, or symbols.";

            return new AuditResult { score = score, feedback = feedback };
        }
    }
}