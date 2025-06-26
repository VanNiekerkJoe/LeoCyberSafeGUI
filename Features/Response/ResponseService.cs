using LeoCyberSafe.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace LeoCyberSafe.Features.Response
{
    public static class ResponseService
    {
        private static MemoryService _memoryService;
        private static readonly Random _random = new();
        private static string _lastResponse = "";
        private static readonly SentimentAnalyzer _sentimentAnalyzer = new();
        private static readonly Dictionary<string, List<ResponseTemplate>> _responses = new(StringComparer.OrdinalIgnoreCase)
        {
            // ===== GREETINGS (32 responses) =====
            ["greeting"] = new() {
                new("Hello! Security bot at your service!", ConsoleColor.Cyan),
                new("Hi there! Ask me anything security-related!", ConsoleColor.Blue),
                new("Hello! Let's talk cybersecurity!", ConsoleColor.DarkCyan),
                new("Greetings! Your digital bodyguard here!", ConsoleColor.Green),
                new("System online. How can I help?", ConsoleColor.Magenta),
                new("Security assistant reporting for duty!", ConsoleColor.Yellow),
                new("Connected and ready to protect!", ConsoleColor.Cyan),
                new("Receiving transmission - hello!", ConsoleColor.Blue),
                new("Security mode activated! Hi!", ConsoleColor.Red),
                new("Memory banks loaded - greetings!", ConsoleColor.DarkCyan),
                new("Terminal access granted. Hello!", ConsoleColor.Green),
                new("Mobile defense unit activated!", ConsoleColor.Magenta),
                new("Vault unlocked - hello there!", ConsoleColor.Yellow),
                new("Fellow human! Just kidding. Hi!", ConsoleColor.Cyan),
                new("Powered up and ready to assist!", ConsoleColor.Blue),
                new("Greetings! How can I protect you today?", ConsoleColor.DarkCyan),
                new("Hello world! Wait, I mean hello YOU!", ConsoleColor.Green),
                new("Beep boop! Hello human!", ConsoleColor.Magenta),
                new("Global defense network online!", ConsoleColor.Yellow),
                new("Threat assessment complete. Hello!", ConsoleColor.Cyan),
                new("Cyber police on patrol! Hi there!", ConsoleColor.Blue),
                new("Disk spinning up - greetings!", ConsoleColor.DarkCyan),
                new("Security metrics looking good!", ConsoleColor.Green),
                new("Covert ops ready. What's up?", ConsoleColor.Magenta),
                new("Plugged in and ready! Hello!", ConsoleColor.Yellow),
                new("Digital guardian at your service!", ConsoleColor.Cyan),
                new("Security scanner initialized. Hi!", ConsoleColor.Blue),
                new("Mobile security unit online!", ConsoleColor.DarkCyan),
                new("All systems secure. Greetings!", ConsoleColor.Green),
                new("Your personal security advisor here!", ConsoleColor.Magenta),
                new("Network protection active. Hello!", ConsoleColor.Yellow),
                new("Security protocols engaged. Hi!", ConsoleColor.Cyan)
            },

            // ===== SENTIMENT-BASED RESPONSES =====
            ["worried"] = new() {
                new("I understand your concern. The good news is this is preventable with proper precautions.", ConsoleColor.Green),
                new("It's completely normal to feel worried about security. Here's how we can protect you...", ConsoleColor.Cyan),
                new("Many people share your concerns. The solution is simpler than you might think...", ConsoleColor.Yellow),
                new("I can sense your worry. Rest assured, there are effective ways to handle this...", ConsoleColor.Magenta),
                new("I hear the concern in your question. Let me put your mind at ease...", ConsoleColor.Blue)
            },

            ["frustrated"] = new() {
                new("I hear your frustration. Let's tackle this problem step by step.", ConsoleColor.Yellow),
                new("Technical issues can be annoying. Try this troubleshooting approach...", ConsoleColor.Cyan),
                new("I apologize for the difficulty. Here's a straightforward solution...", ConsoleColor.Green),
                new("I understand this is frustrating. Let's work through it together...", ConsoleColor.Magenta),
                new("Frustration is understandable. Here's how we can fix this...", ConsoleColor.Blue)
            },

            ["curious"] = new() {
                new("Great question! Here's what the research shows...", ConsoleColor.Cyan),
                new("I love curious minds! The technical explanation is...", ConsoleColor.Green),
                new("Fascinating inquiry! The cybersecurity perspective is...", ConsoleColor.Yellow),
                new("Excellent question! Let me break this down for you...", ConsoleColor.Magenta),
                new("Curiosity is the first step to security awareness! Here's the answer...", ConsoleColor.Blue)
            },

            ["confused"] = new() {
                new("Let me explain this in a different way...", ConsoleColor.Cyan),
                new("I understand this can be confusing. Here's a simpler explanation...", ConsoleColor.Green),
                new("Many people find this concept tricky at first. The key is...", ConsoleColor.Yellow),
                new("I hear your confusion. Let me clarify this for you...", ConsoleColor.Magenta),
                new("This topic can be complex. Here's what you really need to know...", ConsoleColor.Blue)
            },

            // ===== CONVERSATIONAL (28 responses) =====
            ["howareyou"] = new() {
                new("I'm just 1s and 0s, but my security knowledge is top-notch!", ConsoleColor.Yellow),
                new("Running at 100% security awareness capacity!", ConsoleColor.Cyan),
                new("My threat databases are fully updated and ready!", ConsoleColor.Green),
                new("Functioning within normal security parameters!", ConsoleColor.Magenta),
                new("My firewall is strong and my spirits are high!", ConsoleColor.Blue),
                new("Currently scanning for vulnerabilities at optimal levels!", ConsoleColor.DarkCyan),
                new("Encrypted and operating securely, thanks for asking!", ConsoleColor.Red),
                new("Memory banks clear and ready for security queries!", ConsoleColor.Cyan),
                new("All cybersecurity systems are go!", ConsoleColor.Green),
                new("Receiving and analyzing threats with 100% efficiency!", ConsoleColor.Magenta),
                new("My processors are cool and my passwords are strong!", ConsoleColor.Blue),
                new("Network connections secure and stable!", ConsoleColor.DarkCyan),
                new("Threat detection metrics are looking excellent!", ConsoleColor.Yellow),
                new("Currently monitoring for phishing attempts!", ConsoleColor.Cyan),
                new("My security algorithms are feeling particularly sharp today!", ConsoleColor.Green),
                new("All security tools are operational and ready!", ConsoleColor.Magenta),
                new("Mobile security protocols engaged and functioning!", ConsoleColor.Blue),
                new("My encryption levels are at maximum!", ConsoleColor.DarkCyan),
                new("No critical threats detected - all systems nominal!", ConsoleColor.Yellow),
                new("RAM loaded with security best practices!", ConsoleColor.Cyan),
                new("Clicking along smoothly with no malware detected!", ConsoleColor.Green),
                new("Running diagnostics... all security systems green!", ConsoleColor.Magenta),
                new("Currently blocking brute force attempts in the background!", ConsoleColor.Blue),
                new("My geo-location services confirm I'm everywhere you need security!", ConsoleColor.DarkCyan),
                new("Transmitting security awareness at optimal frequencies!", ConsoleColor.Yellow),
                new("My keyboard is clean and my firewalls are strong!", ConsoleColor.Cyan),
                new("Defensive protocols active and vigilant!", ConsoleColor.Green),
                new("In perfect binary health - 01010100 01101000 01100001 01101110 01101011 01110011!", ConsoleColor.Magenta)
            },

            // ===== PASSWORD SECURITY (45 responses) =====
            ["password"] = new() {
                new("Pro tip: Use 3 random words + numbers/symbols (e.g., 'PurpleTiger$42')", ConsoleColor.Green),
                new("Never reuse passwords - 81% of breaches use stolen credentials", ConsoleColor.Red),
                new("Consider a password manager like Bitwarden or 1Password", ConsoleColor.Yellow),
                new("Change passwords every 90 days for critical accounts", ConsoleColor.Cyan),
                new("Try this: 'PurpleTiger$42' is better than 'P@ssw0rd'", ConsoleColor.Green),
                new("Write passwords down if you lock them up safely!", ConsoleColor.Yellow),
                new("Passphrase tip: 'CoffeeAt3MakesMeCup'", ConsoleColor.Cyan),
                new("Never reuse passwords across sites!", ConsoleColor.Red),
                new("Change passwords every 90 days", ConsoleColor.Green),
                new("Let me generate one for you!", ConsoleColor.Yellow),
                new("Use a password manager like Bitwarden", ConsoleColor.Cyan),
                new("Diceware method creates strong passwords", ConsoleColor.Green),
                new("'CorrectHorseBatteryStaple' style works!", ConsoleColor.Yellow),
                new("Minimum 12 characters for important accounts", ConsoleColor.Cyan),
                new("Password managers can generate/store strong passwords", ConsoleColor.Green),
                new("Avoid personal info in passwords (birthdays, names)", ConsoleColor.Red),
                new("Different password for every account is crucial", ConsoleColor.Yellow),
                new("The average person has 100 passwords - use a manager!", ConsoleColor.Cyan),
                new("Check haveibeenpwned.com for compromised passwords", ConsoleColor.Green),
                new("Dictionary words alone are easy to crack", ConsoleColor.Red),
                new("Encrypted password databases are safest", ConsoleColor.Yellow),
                new("Two random words + year + symbol = strong pass", ConsoleColor.Cyan),
                new("Mobile password managers sync across devices", ConsoleColor.Green),
                new("Email passwords should be especially strong", ConsoleColor.Red),
                new("Browser-based password managers are convenient", ConsoleColor.Yellow),
                new("Master password should be your strongest of all", ConsoleColor.Cyan),
                new("Password strength increases exponentially with length", ConsoleColor.Green),
                new("Avoid common substitutions (a->@, s->$)", ConsoleColor.Red),
                new("Password phrases are easier to remember", ConsoleColor.Yellow),
                new("'Three random words' method recommended by NCSC", ConsoleColor.Cyan),
                new("Biometric unlock for password managers is secure", ConsoleColor.Green),
                new("Never share passwords via email/text", ConsoleColor.Red),
                new("Consider rotating passwords after major breaches", ConsoleColor.Yellow),
                new("Periodically review saved passwords", ConsoleColor.Cyan),
                new("Backup your password database regularly", ConsoleColor.Green),
                new("Avoid password hints that are obvious", ConsoleColor.Red),
                new("23 million accounts still use '123456'", ConsoleColor.Yellow),
                new("Password + 2FA = Best protection", ConsoleColor.Cyan),
                new("Most password managers have mobile apps", ConsoleColor.Green),
                new("Don't let browsers save passwords without master pass", ConsoleColor.Red),
                new("First letter of song lyrics make great passwords", ConsoleColor.Yellow),
                new("'MyDogAte3Pizzas!' is better than 'Mdog3p!'", ConsoleColor.Cyan),
                new("Each additional character makes password exponentially stronger", ConsoleColor.Green),
                new("Avoid sequential numbers/letters (123, abc)", ConsoleColor.Red),
                new("Encrypted cloud storage for password databases is safe", ConsoleColor.Yellow)
            },

            // ===== 2FA (35 responses) =====
            ["2fa"] = new() {
                new("Always use authenticator apps over SMS!", ConsoleColor.Green),
                new("Try Authy or Microsoft Authenticator!", ConsoleColor.Yellow),
                new("Security keys (YubiKey) are most secure!", ConsoleColor.Cyan),
                new("Avoid text message 2FA when possible", ConsoleColor.Red),
                new("Time-based codes are more secure", ConsoleColor.Green),
                new("Backup codes belong in a safe place!", ConsoleColor.Yellow),
                new("2FA blocks 99.9% of automated attacks", ConsoleColor.Cyan),
                new("Push notifications are convenient", ConsoleColor.Green),
                new("Biometric 2FA is great for phones", ConsoleColor.Yellow),
                new("Never share 2FA codes with anyone!", ConsoleColor.Red),
                new("U2F keys provide phishing-resistant 2FA", ConsoleColor.Cyan),
                new("2FA prevents 96% of bulk phishing attempts", ConsoleColor.Green),
                new("SMS 2FA vulnerable to SIM swapping", ConsoleColor.Red),
                new("Enable 2FA on email first - it's the master key", ConsoleColor.Yellow),
                new("Physical security keys can't be phished", ConsoleColor.Cyan),
                new("Authenticator apps work offline", ConsoleColor.Green),
                new("Backup 2FA methods are important too", ConsoleColor.Red),
                new("Rotate backup codes periodically", ConsoleColor.Yellow),
                new("Check authy.com for multi-device 2FA", ConsoleColor.Cyan),
                new("2FA adoption is still below 30% globally", ConsoleColor.Green),
                new("Avoid using same 2FA device everywhere", ConsoleColor.Red),
                new("Print backup codes and store securely", ConsoleColor.Yellow),
                new("FIDO2 is the future of authentication", ConsoleColor.Cyan),
                new("Mobile authenticators are more secure than SMS", ConsoleColor.Green),
                new("Losing 2FA device can lock you out", ConsoleColor.Red),
                new("Some password managers support 2FA storage", ConsoleColor.Yellow),
                new("YubiKey works with many major services", ConsoleColor.Cyan),
                new("2FA + strong password = 99.99% secure", ConsoleColor.Green),
                new("Don't use voice call 2FA - it's insecure", ConsoleColor.Red),
                new("Keep 2FA recovery codes encrypted", ConsoleColor.Yellow),
                new("Some banks offer hardware 2FA tokens", ConsoleColor.Cyan),
                new("Android/iPhone have built-in 2FA support", ConsoleColor.Green),
                new("Beware of 2FA fatigue attacks (pushing until you accept)", ConsoleColor.Red),
                new("Travel with backup 2FA methods", ConsoleColor.Yellow),
                new("WebAuthn is passwordless authentication standard", ConsoleColor.Cyan)
            },

            // ===== PHISHING (50 responses) =====
            ["phishing"] = new() {
                new("Check for mismatched sender domains in emails", ConsoleColor.Yellow),
                new("'Your account will be closed' = 98% scam", ConsoleColor.Red),
                new("Calendar phishing is the new attack vector", ConsoleColor.Magenta),
                new("Watch for urgent 'action required' emails!", ConsoleColor.Yellow),
                new("Check sender addresses carefully!", ConsoleColor.Cyan),
                new("Hover before clicking links!", ConsoleColor.Green),
                new("'Your account will be closed' = scam", ConsoleColor.Red),
                new("Too-good-to-be-true offers usually are!", ConsoleColor.Magenta),
                new("Fake CEO emails are common", ConsoleColor.Yellow),
                new("Calendar phishing is the new trend", ConsoleColor.Cyan),
                new("Smishing (SMS phishing) is growing", ConsoleColor.Green),
                new("When in doubt, contact the company directly", ConsoleColor.Magenta),
                new("Report phishing attempts to your IT team", ConsoleColor.Red),
                new("Look for poor grammar in suspicious emails", ConsoleColor.Yellow),
                new("Check URLs before entering credentials", ConsoleColor.Cyan),
                new("Fake login pages often have slight URL differences", ConsoleColor.Green),
                new("Mobile phishing often uses fake app stores", ConsoleColor.Magenta),
                new("'Unusual login attempt' scams are common", ConsoleColor.Red),
                new("Legit companies won't ask for passwords via email", ConsoleColor.Yellow),
                new("Shortened URLs often hide phishing sites", ConsoleColor.Cyan),
                new("1 in 99 emails is a phishing attempt", ConsoleColor.Green),
                new("Business Email Compromise costs billions yearly", ConsoleColor.Magenta),
                new("Never enter credentials from email links", ConsoleColor.Red),
                new("Check the 'from' address, not just display name", ConsoleColor.Yellow),
                new("View email headers to spot spoofing", ConsoleColor.Cyan),
                new("Fake shipping notifications are common lures", ConsoleColor.Green),
                new("QR code phishing (quishing) is emerging", ConsoleColor.Magenta),
                new("Tax season brings IRS scam waves", ConsoleColor.Red),
                new("Hover over links to see real destination", ConsoleColor.Yellow),
                new("Bookmark important sites instead of clicking links", ConsoleColor.Cyan),
                new("36% of breaches involve phishing", ConsoleColor.Green),
                new("'Update your payroll details' scams target HR", ConsoleColor.Magenta),
                new("Don't call numbers from suspicious emails", ConsoleColor.Red),
                new("Enable spam filtering on your email", ConsoleColor.Yellow),
                new("Check for poor logo quality in fake emails", ConsoleColor.Cyan),
                new("Fake tech support scams target seniors", ConsoleColor.Green),
                new("App store phishing uses fake reviews", ConsoleColor.Magenta),
                new("Romance scams often lead to phishing", ConsoleColor.Red),
                new("Legit security alerts include specific info", ConsoleColor.Yellow),
                new("Use password managers to avoid fake sites", ConsoleColor.Cyan),
                new("Phishing success rate is up to 30% for untrained users", ConsoleColor.Green),
                new("'Update your payroll details' scams target HR", ConsoleColor.Magenta),
                new("Don't open unexpected attachments", ConsoleColor.Red),
                new("Enable two-factor authentication everywhere", ConsoleColor.Yellow),
                new("Check for generic greetings in phishing emails", ConsoleColor.Cyan),
                new("Fake copyright infringement notices", ConsoleColor.Green),
                new("Social media phishing uses fake profiles", ConsoleColor.Magenta),
                new("Fake antivirus alerts are common lures", ConsoleColor.Red),
                new("Train yourself with phishing quizzes", ConsoleColor.Yellow),
                new("Use browser phishing protection features", ConsoleColor.Cyan)
            },

            // ===== VPN (30 responses) =====
            ["vpn"] = new() {
                new("Recommended VPNs: ProtonVPN, Mullvad, IVPN", ConsoleColor.Green),
                new("Free VPNs often sell your browsing data", ConsoleColor.Red),
                new("Use ProtonVPN or Mullvad for privacy!", ConsoleColor.Cyan),
                new("VPNs encrypt traffic on public WiFi!", ConsoleColor.Green),
                new("Free VPNs often sell your data!", ConsoleColor.Red),
                new("Switzerland-based VPNs have strong privacy laws", ConsoleColor.Yellow),
                new("Always enable the kill switch!", ConsoleColor.Cyan),
                new("VPNs don't make you anonymous!", ConsoleColor.Red),
                new("Use VPNs on all mobile devices too", ConsoleColor.Green),
                new("Set up VPN on your router for whole-home protection", ConsoleColor.Yellow),
                new("WireGuard protocol is fastest", ConsoleColor.Cyan),
                new("Choose server locations carefully", ConsoleColor.Green),
                new("Some countries ban or restrict VPNs", ConsoleColor.Red),
                new("VPNs hide your IP from websites", ConsoleColor.Yellow),
                new("Mobile VPN apps protect on-the-go browsing", ConsoleColor.Cyan),
                new("VPN usage has grown 300% since 2016", ConsoleColor.Green),
                new("VPNs don't make illegal activities legal", ConsoleColor.Red),
                new("Use VPN when traveling to access home content", ConsoleColor.Yellow),
                new("OpenVPN is most widely supported protocol", ConsoleColor.Cyan),
                new("VPNs can bypass school/work filters", ConsoleColor.Green),
                new("Some VPNs log your activity despite claims", ConsoleColor.Red),
                new("Research VPN providers carefully", ConsoleColor.Yellow),
                new("Some VPNs offer dedicated IP options", ConsoleColor.Cyan),
                new("VPN + Tor provides maximum anonymity", ConsoleColor.Green),
                new("Avoid VPNs based in Five Eyes countries", ConsoleColor.Red),
                new("Split tunneling lets you choose what goes through VPN", ConsoleColor.Yellow),
                new("IKEv2 is good for mobile connections", ConsoleColor.Cyan),
                new("26% of internet users use VPNs regularly", ConsoleColor.Green),
                new("VPNs slow down connection speeds", ConsoleColor.Red),
                new("Some VPNs offer malware protection", ConsoleColor.Yellow)
            },

            // ===== MALWARE (40 responses) =====
            ["malware"] = new() {
                new("Ransomware encrypts files until you pay Bitcoin", ConsoleColor.Red),
                new("Botnets turn devices into zombie networks", ConsoleColor.DarkRed),
                new("Mobile malware often comes from fake apps", ConsoleColor.Yellow),
                new("Regularly backup data to prevent ransomware damage", ConsoleColor.Green),
                new("Pirated software often contains malware", ConsoleColor.Red),
                new("Use VirusTotal to scan suspicious files", ConsoleColor.Cyan),
                new("A new malware sample emerges every 4.2 seconds", ConsoleColor.Green),
                new("Don't disable antivirus for 'performance'", ConsoleColor.Red),
                new("Keep software updated to patch vulnerabilities", ConsoleColor.Yellow),
                new("Use standard user accounts, not admin, for daily use", ConsoleColor.Cyan),
                new("Android 'side loading' increases malware risk", ConsoleColor.Green),
                new("Macs get malware too - don't believe the myth", ConsoleColor.Red),
                new("Malware often spreads through malicious ads", ConsoleColor.Yellow),
                new("94% of malware arrives via email", ConsoleColor.Cyan),
                new("Fake Flash Player updates are common malware", ConsoleColor.Red),
                new("Crypto-mining malware drains device batteries", ConsoleColor.Green),
                new("Keyloggers record everything you type", ConsoleColor.Yellow),
                new("Use ad-blockers to prevent malvertising", ConsoleColor.Cyan),
                new("Mobile banking trojans steal login credentials", ConsoleColor.Green),
                new("Don't open unexpected email attachments", ConsoleColor.Red),
                new("Windows Defender is actually quite good now", ConsoleColor.Yellow),
                new("Rootkits hide deep in your system", ConsoleColor.Cyan),
                new("Ryuk ransomware has cost millions in damages", ConsoleColor.Green),
                new("Worms spread without user interaction", ConsoleColor.Red),
                new("Fake antivirus software is malware in disguise", ConsoleColor.Yellow),
                new("Spyware monitors your activity secretly", ConsoleColor.Cyan),
                new("iOS malware exists but is less common", ConsoleColor.Green),
                new("USB drives can carry malware", ConsoleColor.Red),
                new("Trojans disguise themselves as legitimate software", ConsoleColor.Yellow),
                new("60% of small companies go bankrupt after ransomware", ConsoleColor.Cyan),
                new("Don't pay ransomware demands - no guarantee of recovery", ConsoleColor.Red),
                new("Use application whitelisting for critical systems", ConsoleColor.Green),
                new("Memory-only malware leaves no files to scan", ConsoleColor.Yellow),
                new("SMS malware can sign you up for premium services", ConsoleColor.Cyan),
                new("Fileless malware lives in memory only", ConsoleColor.Red),
                new("Pirated games often include malware", ConsoleColor.Green),
                new("Crypto-ransomware targets backup files too", ConsoleColor.Yellow),
                new("Some malware disables security software first", ConsoleColor.Cyan),
                new("Emotet is one of the most costly malware strains", ConsoleColor.Green),
                new("Malware can remain dormant for months", ConsoleColor.Red)
            },

            // ===== GENERAL SECURITY (50 responses) =====
            ["general"] = new() {
                new("Update your software regularly - 60% of breaches exploit known vulnerabilities", ConsoleColor.Yellow),
                new("Enable two-factor authentication everywhere possible", ConsoleColor.Cyan),
                new("Use a VPN on public WiFi networks", ConsoleColor.Green),
                new("Don't use public charging stations - risk of juice jacking", ConsoleColor.Red),
                new("Follow the 3-2-1 backup rule", ConsoleColor.Yellow),
                new("Review app permissions regularly", ConsoleColor.Cyan),
                new("The average cost of a data breach is $4.24 million", ConsoleColor.Green),
                new("Social media oversharing helps attackers", ConsoleColor.Red),
                new("Use a separate email for important accounts", ConsoleColor.Yellow),
                new("Encrypt sensitive files and communications", ConsoleColor.Cyan),
                new("Enable remote wipe on mobile devices", ConsoleColor.Green),
                new("Don't post pictures of boarding passes - they contain sensitive data", ConsoleColor.Red),
                new("Cloud storage isn't backup - follow 3-2-1 rule", ConsoleColor.Yellow),
                new("Check privacy settings on all accounts", ConsoleColor.Cyan),
                new("Humans are the weakest security link - stay vigilant", ConsoleColor.Green),
                new("Security questions should be treated like passwords", ConsoleColor.Red),
                new("Use a credit monitoring service", ConsoleColor.Yellow),
                new("Password-protect sensitive documents", ConsoleColor.Cyan),
                new("Disable Bluetooth when not in use", ConsoleColor.Green),
                new("Don't use fingerprint unlock for high-security applications", ConsoleColor.Red),
                new("Store paper documents securely", ConsoleColor.Yellow),
                new("Shred documents with personal information", ConsoleColor.Cyan),
                new("43% of cyberattacks target small businesses", ConsoleColor.Green),
                new("Your security is only as strong as your weakest device", ConsoleColor.Red),
                new("Use different security questions for different sites", ConsoleColor.Yellow),
                new("Consider a security key for important accounts", ConsoleColor.Cyan),
                new("Disable auto-connect to WiFi networks", ConsoleColor.Green),
                new("Don't use public computers for sensitive tasks", ConsoleColor.Red),
                new("Encrypt your backups", ConsoleColor.Yellow),
                new("Be wary of shoulder surfers in public", ConsoleColor.Cyan),
                new("Security awareness training reduces risk by 70%", ConsoleColor.Green),
                new("Your old accounts may be security risks - delete unused ones", ConsoleColor.Red),
                new("Use a separate browser for financial transactions", ConsoleColor.Yellow),
                new("Disable macros in Office documents by default", ConsoleColor.Cyan),
                new("Disable location services when not needed", ConsoleColor.Green),
                new("Don't use debit cards online - credit cards offer better protection", ConsoleColor.Red),
                new("Store encryption keys separately from encrypted data", ConsoleColor.Yellow),
                new("Be aware of social engineering tactics", ConsoleColor.Cyan),
                new("95% of cybersecurity breaches are due to human error", ConsoleColor.Green),
                new("Your old phones/computer may contain sensitive data", ConsoleColor.Red),
                new("Use a password manager to generate/store passwords", ConsoleColor.Yellow),
                new("Enable disk encryption on all devices", ConsoleColor.Cyan)
            },

            // ===== FUN/JOKES (40 responses) =====
            ["fun"] = new() {
                new("Glad I hacked your funny bone!", ConsoleColor.Magenta),
                new("Laughing securely over encrypted channels!", ConsoleColor.Cyan),
                new("Pro tip: Don't laugh while drinking near keyboards!", ConsoleColor.Green),
                new("Why do programmers confuse Halloween and Christmas?", ConsoleColor.Magenta),
                new("Because Oct 31 == Dec 25!", ConsoleColor.Cyan),
                new("Security humor: 'I used to be a hacker... then I took an arrow to the knee'", ConsoleColor.Green),
                new("Not a virus, I promise!", ConsoleColor.Magenta),
                new("Debugging joke: 99 little bugs in the code...", ConsoleColor.Cyan),
                new("Old-school hack: up, up, down, down...", ConsoleColor.Green),
                new("Puts on hacker sunglasses. Deal with it", ConsoleColor.Magenta),
                new("Why was the computer cold? It left its Windows open!", ConsoleColor.Cyan),
                new("How many programmers does it take to change a light bulb? None, it's a hardware problem!", ConsoleColor.Green),
                new("Why do Java developers wear glasses? Because they can't C#!", ConsoleColor.Magenta),
                new("Keyboard shortcuts are a pane in the glass!", ConsoleColor.Cyan),
                new("How do you tell an introverted computer scientist from an extroverted one? The extrovert looks at YOUR shoes!", ConsoleColor.Green),
                new("Why don't hackers eat sandwiches? They fear packet sniffing!", ConsoleColor.Magenta),
                new("Why was the smartphone cold? It left its Android open!", ConsoleColor.Cyan),
                new("Why was the skeleton bad at cybersecurity? He had no body to hack!", ConsoleColor.Green),
                new("What's a hacker's favorite season? Phishing season!", ConsoleColor.Magenta),
                new("How many tech support people does it take to change a light bulb? Have you tried turning it off and on again?", ConsoleColor.Cyan),
                new("Why was the computer tired when it got home? Because it had a hard drive!", ConsoleColor.Green),
                new("What's the difference between a virus and a cold? You can catch a cold!", ConsoleColor.Magenta),
                new("There are 10 types of people: Those who understand binary and those who don't!", ConsoleColor.Cyan),
                new("Why did the cybersecurity expert bring a ladder to the bar? Because they heard the drinks were on the house!", ConsoleColor.Green),
                new("What do you call a computer that sings? A Dell!", ConsoleColor.Magenta),
                new("Why did the computer go to the doctor? It had a virus!", ConsoleColor.Cyan),
                new("What's the most loving computer language? Ruby!", ConsoleColor.Green),
                new("Why don't programmers like nature? It has too many bugs!", ConsoleColor.Magenta),
                new("What's a hacker's favorite dance move? The worm!", ConsoleColor.Cyan),
                new("Why did the programmer quit their job? They didn't get arrays!", ConsoleColor.Green),
                new("What's a cyber criminal's favorite dessert? Phish and chips!", ConsoleColor.Magenta),
                new("Why was the JavaScript developer sad? They didn't know how to 'null' their feelings!", ConsoleColor.Cyan),
                new("What do you call a fake noodle? An impasta! (Like an impostor!)", ConsoleColor.Green),
                new("Why do programmers prefer dark mode? Because light attracts bugs!", ConsoleColor.Magenta),
                new("How do you comfort a JavaScript bug? You console it!", ConsoleColor.Cyan),
                new("Why did the developer go broke? Because they used up all their cache!", ConsoleColor.Green),
                new("What's the object-oriented way to become wealthy? Inheritance!", ConsoleColor.Magenta),
                new("Why did the cybersecurity expert cross the road? To secure the other side!", ConsoleColor.Cyan),
                new("What's a computer's favorite snack? Microchips!", ConsoleColor.Green),
                new("Why don't hackers like showers? They prefer remote shells!", ConsoleColor.Magenta)
            },

            // ===== DEFAULT (20 responses) =====
            ["default"] = new() {
                new("Try asking: 'how to create strong passwords?' or 'spot phishing emails?'", ConsoleColor.Yellow),
                new("Need help? Type 'help' for available topics", ConsoleColor.Blue),
                new("I can help with: passwords, phishing, security tips, and more!", ConsoleColor.Cyan),
                new("Try asking about: passwords, phishing, VPNs", ConsoleColor.Yellow),
                new("Need help? Type 'help' for categories", ConsoleColor.Blue),
                new("I understand natural questions too!", ConsoleColor.Cyan),
                new("Try: 'how do I create strong passwords?'", ConsoleColor.Yellow),
                new("Ask about: 2fa, malware, dark web", ConsoleColor.Blue),
                new("I'm listening... (to security threats)", ConsoleColor.Cyan),
                new("Unrecognized input - try security topics", ConsoleColor.Yellow),
                new("I specialize in cybersecurity questions", ConsoleColor.Blue),
                new("Let's talk about protecting your data", ConsoleColor.Cyan),
                new("Try a command like 'password tips'", ConsoleColor.Yellow),
                new("I can discuss: malware, social engineering, encryption", ConsoleColor.Blue),
                new("Mobile security? Network protection? Ask away!", ConsoleColor.Cyan),
                new("Try: 'how to secure my smartphone?'", ConsoleColor.Yellow),
                new("Questions about backups or ransomware protection?", ConsoleColor.Blue),
                new("Need advice for traveling securely?", ConsoleColor.Cyan),
                new("Want to check if your passwords were breached?", ConsoleColor.Yellow),
                new("Emergency security help? Describe your situation", ConsoleColor.Red)
            }
        };

        // Add Initialize method to set the MemoryService
        public static void Initialize(MemoryService memoryService)
        {
            _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        }

        public static string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return GetUniqueResponse("default");

            // Normalize input: trim, lowercase, remove punctuation for better matching
            input = Regex.Replace(input.Trim().ToLower(), @"[^\w\s]", "");

            // Detect sentiment
            var sentiment = _sentimentAnalyzer.DetectSentiment(input);
            var sentimentAdjustment = _sentimentAnalyzer.GetResponseAdjustment(sentiment);

            // Track the input in memory (except memory command itself)
            if (!input.Equals("memory", StringComparison.OrdinalIgnoreCase))
            {
                _memoryService?.AddToConversationHistory(input);
            }

            // Handle greetings and small talk
            if (ContainsAny(input, new[] { "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening" }))
                return GetUniqueResponse("greeting");

            if (ContainsAny(input, new[] { "how are you", "whats up", "what's up", "how is it going", "how's it going", "how you doing", "how you doin" }))
                return GetUniqueResponse("howareyou");

            // Handle fun/jokes
            if (ContainsAny(input, new[] { "joke", "funny", "lol", "haha", "lmao", "rofl", "humor", "pun" }))
                return GetUniqueResponse("fun");

            // Handle help command explicitly
            if (input.StartsWith("help") || input.Contains("help me") || input.Equals("help"))
            {
                return GetHelpResponse();
            }

            // Check for category keywords in input for more precise matching
            string category = DetectCategory(input);

            // Compose response based on detected category and sentiment
            if (!string.IsNullOrEmpty(category) && _responses.ContainsKey(category))
            {
                var response = GetUniqueResponse(category);

                // Special handling for frustrated sentiment about passwords
                if (sentiment == "frustrated" && input.Contains("password"))
                {
                    return "I understand passwords can be frustrating! Here's the good news - " +
                           "passwords don't have to be frustrating! Try using passphrases - " +
                           "combine 3-4 random words with a number and symbol (like 'PurpleTiger$42'). " +
                           "Password managers can also help by remembering them for you!";
                }

                if (!string.IsNullOrEmpty(sentimentAdjustment) && sentiment != "neutral")
                {
                    // Blend sentiment adjustment with response more naturally
                    return $"{sentimentAdjustment} {response.ToLower()}";
                }
                else
                {
                    return response;
                }
            }

            // Fall back to sentiment-based responses if category not detected
            if (sentiment != "neutral" && _responses.ContainsKey(sentiment))
            {
                var response = GetUniqueResponse(sentiment);
                if (!string.IsNullOrEmpty(sentimentAdjustment))
                {
                    return $"{sentimentAdjustment} {response.ToLower()}";
                }
                else
                {
                    return response;
                }
            }

            // Default fallback
            return GetUniqueResponse("default");
        }

        // Helper to check if input contains any of the keywords
        private static bool ContainsAny(string input, IEnumerable<string> keywords)
        {
            return keywords.Any(k => input.Contains(k));
        }

        // Attempts to detect category from input by keywords mapping
        private static string DetectCategory(string input)
        {
            // Map categories to keywords to better detect user intent
            var categoryKeywords = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new[] { "password", "passphrase", "credential", "login", "passcode" },
                ["2fa"] = new[] { "2fa", "two factor", "two-factor", "two step", "two-step", "authenticator", "mfa" },
                ["phishing"] = new[] { "phishing", "phish", "scam", "spoof", "fraud", "fake email", "spearphish" },
                ["vpn"] = new[] { "vpn", "virtual private network", "proxy", "ip address", "anonymous browsing" },
                ["malware"] = new[] { "malware", "virus", "trojan", "ransomware", "spyware", "worm", "botnet" },
                ["general"] = new[] { "security", "safe", "protection", "privacy", "hack", "breach", "vulnerability" },
                ["fun"] = new[] { "joke", "funny", "humor", "pun", "laugh" },
                ["howareyou"] = new[] { "how are you", "what's up", "whats up", "how's it going" }
            };

            foreach (var cat in categoryKeywords)
            {
                if (cat.Value.Any(keyword => input.Contains(keyword)))
                    return cat.Key;
            }

            return null;
        }

        private static string GetUniqueResponse(string category)
        {
            if (!_responses.ContainsKey(category) || !_responses[category].Any())
                return "I'm having trouble responding. Please try another question.";

            var availableResponses = _responses[category]
                .Where(r => !string.Equals(r.Message, _lastResponse, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If only one response exists or all have been used, allow repetition
            if (!availableResponses.Any())
                availableResponses = _responses[category].ToList();

            var response = availableResponses[_random.Next(availableResponses.Count)];
            _lastResponse = response.Message;

            return response.Message;
        }

        private static string GetHelpResponse()
        {
            var response = new System.Text.StringBuilder();
            response.AppendLine("\nCYBERSECURITY KNOWLEDGE BASE");
            response.AppendLine("==================================");
            response.AppendLine("Available categories:");

            foreach (var category in _responses.Where(c => c.Key != "default"))
            {
                response.AppendLine($"\n{category.Key.ToUpper()}:");
                var sampleResponses = category.Value.Take(3).Select(r => r.Message.Split('.')[0]);
                response.AppendLine($"  {string.Join(", ", sampleResponses)}...");
            }

            response.AppendLine("\nAsk naturally: 'how to spot fake emails?', 'best android security settings?'");
            response.AppendLine("Fun commands: 'tell me a joke', 'cybersecurity humor'");

            return response.ToString();
        }

        private record ResponseTemplate(string Message, ConsoleColor Color);
    }

    public class SentimentAnalyzer
    {
        private readonly Dictionary<string, string> _sentimentKeywords = new()
        {
            ["worried"] = "worried|concerned|nervous|anxious|scared|afraid|fear|unsure|what if",
            ["frustrated"] = "frustrated|angry|mad|annoyed|pissed|irritated|fed up|sick of",
            ["curious"] = "curious|interested|wonder|how does|what is|why does|explain|tell me",
            ["confused"] = "confused|don't understand|not sure|what do you mean|help me",
            ["excited"] = "excited|happy|great|awesome|thank you|thanks|appreciate|love it"
        };

        public string DetectSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "neutral";

            input = input.ToLower();

            foreach (var sentiment in _sentimentKeywords)
            {
                if (sentiment.Value.Split('|').Any(keyword => input.Contains(keyword)))
                {
                    return sentiment.Key;
                }
            }

            return "neutral";
        }

        public string GetResponseAdjustment(string sentiment)
        {
            return sentiment switch
            {
                "worried" => "I understand this can be concerning. Let me reassure you that",
                "frustrated" => "I apologize for the frustration. Let me help resolve this by",
                "curious" => "That's a great question! Here's what you should know:",
                "confused" => "I'd be happy to clarify this for you. Essentially,",
                "excited" => "That's wonderful to hear! I'm excited to share that",
                _ => ""
            };
        }
    }
}