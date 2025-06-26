using System;
using System.Collections.Generic;
using System.Linq;

namespace LeoCyberSafe.Features.Questions
{
    public class QuestionService
    {
        private readonly Dictionary<string, string> _qaPairs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = "Phishing is a cyberattack using disguised emails to trick recipients into revealing sensitive information.",
            ["strong password"] = "Create passwords with:\n- 12+ characters\n- Upper/lower case letters\n- Numbers\n- Symbols\nExample: 'PurpleMountain$42'",
            ["malware"] = "Malicious software including viruses, worms, and ransomware that can damage your devices.",
            ["vpn"] = "A VPN encrypts your internet connection and hides your IP address for secure browsing.",
            ["ransomware"] = "Ransomware encrypts your files until you pay. Prevent it with regular backups.",
            ["2fa"] = "Two-factor authentication adds an extra security step beyond just passwords."
        };

        public string GetAnswer(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Please ask a cybersecurity question.";

            question = question.Trim().ToLower();

            // Check for direct matches first
            foreach (var pair in _qaPairs)
            {
                if (question.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            // Check for similar questions
            var similarQuestions = _qaPairs.Keys
                .Where(key => ComputeSimilarity(question, key) > 0.6)
                .ToList();

            if (similarQuestions.Count > 0)
            {
                return $"About '{similarQuestions[0]}':\n{_qaPairs[similarQuestions[0]]}";
            }

            return GetFallbackResponse();
        }

        private double ComputeSimilarity(string a, string b)
        {
            var wordsA = new HashSet<string>(a.Split(new[] { ' ', '?', '!' }, StringSplitOptions.RemoveEmptyEntries));
            var wordsB = new HashSet<string>(b.Split(new[] { ' ', '?', '!' }, StringSplitOptions.RemoveEmptyEntries));

            var intersection = wordsA.Intersect(wordsB).Count();
            var union = wordsA.Union(wordsB).Count();

            return union == 0 ? 0 : (double)intersection / union;
        }

        private string GetFallbackResponse()
        {
            var random = new Random();
            var suggestions = _qaPairs.Keys
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();

            return "I'm not sure I understand. Try asking about:\n" +
                   string.Join("\n", suggestions.Select(s => $"• {s}")) +
                   "\nOr say 'topics' to see all available questions.";
        }

        public void DisplayAvailableTopics()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nI can answer questions about:");
            Console.WriteLine(string.Join(", ", _qaPairs.Keys.OrderBy(k => k)));
            Console.ResetColor();
        }
    }
    }
