using System;

namespace LeoCyberSafe.Features.Phishing
{
    public class PhishingSimulator
    {
        public void StartSimulation(string userName)
        {
            Console.WriteLine($"Phishing Simulation for {userName} started at {DateTime.Now:MM/dd/yyyy hh:mm tt SAST}");
            Console.WriteLine("Simulated email: 'Urgent: Click here to verify your account!'");
            Console.WriteLine("Action: User should report this email. Simulation complete.");
        }
    }
}