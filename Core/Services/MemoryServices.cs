using LeoCyberSafe.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeoCyberSafe.Core.Services
{
    public class MemoryService
    {
        private readonly UserMemory _memory;

        public MemoryService(UserMemory memory)
        {
            _memory = memory;
        }

        public void DisplayMemory()
        {
            var history = _memory.GetConversationHistory();
            var frequency = _memory.GetQuestionFrequency();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== CONVERSATION HISTORY ===");
            Console.ResetColor();

            if (history.Count == 0)
            {
                Console.WriteLine("No conversation history yet.");
                return;
            }

            Console.WriteLine("\nRecent prompts (newest first):");
            foreach (var item in history.TakeLast(5).Reverse())
            {
                Console.WriteLine($"- {item}");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== MOST FREQUENT QUESTIONS ===");
            Console.ResetColor();

            var topQuestions = frequency
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .OrderByDescending(x => x.Value)
                .Take(3);

            if (!topQuestions.Any())
            {
                Console.WriteLine("No frequent questions yet.");
                return;
            }

            foreach (var question in topQuestions)
            {
                Console.WriteLine($"- \"{question.Key}\" (asked {question.Value} time{(question.Value > 1 ? "s" : "")})");
            }
        }

        public void AddToConversationHistory(string input)
        {
            _memory.AddToConversationHistory(input);
        }

        public List<string> GetConversationHistory()
        {
            return _memory.GetConversationHistory();
        }

        public Dictionary<string, int> GetQuestionFrequency()
        {
            return _memory.GetQuestionFrequency();
        }
    }
}