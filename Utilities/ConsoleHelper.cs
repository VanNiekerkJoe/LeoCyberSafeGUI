using LeoCyberSafe.Core.Models;
using System;

namespace LeoCyberSafe.Utilities
{
    public static class ConsoleHelper
    {
        public static void DisplayAsciiArt()
        {
            Console.WriteLine(@"
            ██████╗  █████╗ ██████╗ ███████╗
            ██╔══██╗██╔══██╗██╔══██╗██╔════╝
            ██████╔╝███████║██████╔╝███████╗
            ██╔══██╗██╔══██║██╔══██╗╚════██║
            ██████╔╝██║  ██║██║  ██║███████║
            ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝
            ");
            Console.WriteLine("Welcome to Leo CyberSafe v3.1 - Your Cybersecurity Assistant!");
        }

        public static void PrintReport(ThreatScanner.ThreatReport report)
        {
            Console.WriteLine($"Threat Report - {DateTime.Now:MM/dd/yyyy hh:mm tt SAST}");
            Console.WriteLine("Threats Detected:");
            foreach (var threat in report.Threats)
            {
                Console.WriteLine($"- {threat}");
            }
            Console.WriteLine($"Severity Score: {report.SeverityScore}/100");
        }
    }
}