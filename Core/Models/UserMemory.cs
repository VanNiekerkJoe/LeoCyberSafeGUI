using System;
using System.Collections.Generic;
using System.Linq;

namespace LeoCyberSafe.Core.Models
{
    public class UserMemory
    {
        public string Name { get; set; }
        public List<string> Interests { get; } = new List<string>();
        public string CurrentTopic { get; set; }
        private List<string> _conversationHistory = new List<string>();
        private Dictionary<string, string> _preferences = new Dictionary<string, string>();
        private Dictionary<string, int> _questionFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void RememberInterest(string interest)
        {
            if (!string.IsNullOrWhiteSpace(interest) && !Interests.Contains(interest))
            {
                Interests.Add(interest);
            }
        }

        public void RecallInterests()
        {
            if (Interests.Count == 0)
            {
                Console.WriteLine("You have no recorded interests.");
                return;
            }

            Console.WriteLine("Your recorded interests:");
            foreach (var interest in Interests)
            {
                Console.WriteLine($"- {interest}");
            }
        }

        public void AddToConversationHistory(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                _conversationHistory.Add(input);

                var normalizedInput = input.Trim();
                if (_questionFrequency.ContainsKey(normalizedInput))
                {
                    _questionFrequency[normalizedInput]++;
                }
                else
                {
                    _questionFrequency[normalizedInput] = 1;
                }
            }
        }

        public List<string> GetConversationHistory()
        {
            return new List<string>(_conversationHistory);
        }

        public Dictionary<string, int> GetQuestionFrequency()
        {
            return new Dictionary<string, int>(_questionFrequency);
        }

        public string GetMostFrequentTopic()
        {
            if (_conversationHistory.Count == 0)
                return "No conversation history available.";

            var mostFrequent = _conversationHistory
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return mostFrequent?.Key ?? "No topics found.";
        }

        public string GetPreference(string key)
        {
            _preferences.TryGetValue(key, out var value);
            return value;
        }

        public void SetPreference(string key, string value)
        {
            if (value == null)
            {
                _preferences.Remove(key);
            }
            else
            {
                _preferences[key] = value;
            }
        }
    }
}