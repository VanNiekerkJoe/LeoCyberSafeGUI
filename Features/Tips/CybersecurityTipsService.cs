using System;
using System.Collections.Generic;

namespace LeoCyberSafe.Features.Tips
{
    public class CybersecurityTipsService
    {
        private readonly Dictionary<string, List<string>> _categorizedTips = new()
        {
            ["passwords"] = new List<string>
            {
                " Use passphrases instead of passwords (e.g., 'PurpleMountain$unset42')",
                " Enable two-factor authentication everywhere possible",
                " Never reuse passwords across different sites"
            },
            ["phishing"] = new List<string>
            {
                " Check email sender addresses carefully",
                " Hover over links before clicking to see the real URL",
                " Be wary of urgent/too-good-to-be-true offers"
            },
            ["general"] = new List<string>
            {
                " Update your software regularly",
                " Use a VPN on public WiFi networks",
                " Backup important data using the 3-2-1 rule",
                " Review app permissions on your devices monthly"
            }
        };

        public void DisplayRandomTip()
        {
            var allTips = new List<string>();
            foreach (var category in _categorizedTips.Values)
            {
                allTips.AddRange(category);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n" + allTips[new Random().Next(allTips.Count)]);
            Console.ResetColor();
        }

        public void DisplayTipsByCategory(string category)
        {
            if (_categorizedTips.TryGetValue(category.ToLower(), out var tips))
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n🔒 {category.ToUpper()} TIPS:");
                foreach (var tip in tips)
                {
                    Console.WriteLine($"- {tip}");
                }
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Category not found. Try: passwords, phishing, general");
            }
        }
    }
}