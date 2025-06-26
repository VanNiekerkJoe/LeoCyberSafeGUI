using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LeoCyberSafe.Core.Models;
using LeoCyberSafe.Core.Services;
using LeoCyberSafe.Features.Password;
using LeoCyberSafe.Features.Phishing;
using LeoCyberSafe.Features.Questions;
using LeoCyberSafe.Features.Response;
using LeoCyberSafe.Features.SecureNotes;
using LeoCyberSafe.Features.Tips;
using LeoCyberSafe.Utilities;

namespace LeoCyberSafeGUI
{
    public partial class MainWindow : Window
    {
        public class TaskItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReminderTime { get; set; }
            public bool IsCompleted { get; set; }
        }

        public class ActivityLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Action { get; set; }
            public string Details { get; set; }
        }

        private class QuizQuestion
        {
            public string Text { get; }
            public List<string> Options { get; }
            public int CorrectIndex { get; }

            public QuizQuestion(string text, List<string> options, int correctIndex)
            {
                Text = text;
                Options = options;
                CorrectIndex = correctIndex;
            }
        }

        private readonly List<QuizQuestion> _quizQuestions = new()
        {
            new("What should you do if you receive an email asking for your password?",
                new List<string> { "Reply with password", "Delete email", "Report as phishing", "Ignore" },
                2),
            new("Which is a sign of a phishing email?",
                new List<string> { "Official logo", "Urgent tone", "Personal greeting", "Clear sender" },
                1),
            new("What makes a strong password?",
                new List<string> { "Short length", "Personal info", "12+ chars with symbols", "Common words" },
                2),
            new("Two-factor authentication adds:",
                new List<string> { "Extra passwords", "Security questions", "Additional verification step", "Encryption" },
                2),
            new("VPNs protect your:",
                new List<string> { "Hardware", "Internet connection", "Passwords", "Email attachments" },
                1),
            new("Malware can spread via:",
                new List<string> { "Email attachments", "Security updates", "VPNs", "2FA codes" },
                0),
            new("Public WiFi risks include:",
                new List<string> { "Fast speeds", "Data interception", "Free access", "No passwords" },
                1),
            new("Password managers help with:",
                new List<string> { "Creating weak passwords", "Remembering passwords", "Sharing passwords", "Ignoring 2FA" },
                1),
            new("Social engineering attacks:",
                new List<string> { "Exploit software bugs", "Manipulate people", "Use encryption", "Require coding" },
                1),
            new("Backups should follow the:",
                new List<string> { "1-2-3 rule", "3-2-1 rule", "2-4-6 rule", "5-5-5 rule" },
                1)
        };

        private int _currentQuestionIndex = -1;
        private int _quizScore = 0;
        private readonly List<ActivityLogEntry> _activityLog = new();
        private readonly List<TaskItem> _tasks = new();
        private readonly Random _random = new();
        private readonly ThreatScanner _threatScanner = new();
        private UserMemory _userMemory;
        private MemoryService _memoryService;
        private CybersecurityTipsService _tipsService;
        private QuestionService _questionService;
        private bool _isNightMode = false;
        private DispatcherTimer _typingTimer;
        private string _currentBotResponse = "";
        private int _currentCharIndex = 0;
        private DispatcherTimer _reminderTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            InitializeTheme();
            InitializeReminderTimer();
        }

        private void InitializeServices()
        {
            _userMemory = new UserMemory();
            _memoryService = new MemoryService(_userMemory);
            _tipsService = new CybersecurityTipsService();
            _questionService = new QuestionService();
            ResponseService.Initialize(_memoryService);
        }

        private void InitializeReminderTimer()
        {
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _reminderTimer.Tick += (s, e) => CheckReminders();
            _reminderTimer.Start();
        }

        private void InitializeTheme()
        {
            Application.Current.Resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF6, 0xF5));
            Application.Current.Resources["ContentBackground"] = Brushes.White;
            Application.Current.Resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x3E, 0x50));
            Application.Current.Resources["StatusBackground"] = new SolidColorBrush(Color.FromRgb(0xEC, 0xF0, 0xF1));
            Application.Current.Resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(0xBD, 0xC3, 0xC7));
            Application.Current.Resources["TextForeground"] = Brushes.Black;
            Application.Current.Resources["ButtonStyle"] = this.Resources["LightButton"];
            Application.Current.Resources["TextBoxStyle"] = this.Resources["LightTextBox"];
            Application.Current.Resources["ListBoxStyle"] = this.Resources["LightListBox"];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AsciiArtHeader.Text = @"
██████╗ ██╗   ██╗    ██╗     ███████╗ ██████╗ 
██╔══██╗╚██╗ ██╔╝    ██║     ██╔════╝██╔═══██╗
██████╔╝ ╚████╔╝     ██║     █████╗  ██║   ██║
██╔══██╗  ╚██╔╝      ██║     ██╔══╝  ██║   ██║
██████╔╝   ██║       ███████╗███████╗╚██████╔╝
╚═════╝    ╚═╝       ╚══════╝╚══════╝ ╚═════╝                    
";

            try
            {
                AudioHelper.PlayWelcomeSound();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Audio error: {ex.Message}");
            }

            StartTypingResponse("Hello! Welcome to Leo CyberSafe - Your Cybersecurity Assistant!\n");
            StartTypingResponse("I can help you with:\n- Security questions\n- Password audits\n- Threat scans\n- Secure notes\n- Cybersecurity quiz\n\n");

            InitializeSecureNotes();
            CheckReminders();
            LogActivity("System", "Application started");
        }

        private void InitializeSecureNotes()
        {
            try
            {
                string masterPassword = "securePassword123";
                NoteService.Initialize(masterPassword);
            }
            catch (Exception ex)
            {
                UpdateChatDisplay($"⚠️ Secure Notes initialization failed: {ex.Message}\n");
            }
        }

        private void StartTypingResponse(string response)
        {
            _currentBotResponse = response;
            _currentCharIndex = 0;

            if (_typingTimer != null)
            {
                _typingTimer.Stop();
            }

            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(30);
            _typingTimer.Tick += TypingTimer_Tick;
            _typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentCharIndex < _currentBotResponse.Length)
            {
                ChatDisplay.AppendText(_currentBotResponse[_currentCharIndex].ToString());
                _currentCharIndex++;
                ChatDisplay.ScrollToEnd();
            }
            else
            {
                _typingTimer.Stop();
                ChatDisplay.AppendText("\n");
            }
        }

        private void NightModeButton_Click(object sender, RoutedEventArgs e)
        {
            _isNightMode = !_isNightMode;

            if (_isNightMode)
            {
                NightModeButton.Content = "☀️ Day Mode";
                Application.Current.Resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30));
                Application.Current.Resources["ContentBackground"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
                Application.Current.Resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x23, 0x7E));
                Application.Current.Resources["StatusBackground"] = new SolidColorBrush(Color.FromRgb(0x26, 0x32, 0x38));
                Application.Current.Resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61));
                Application.Current.Resources["TextForeground"] = Brushes.White;
                Application.Current.Resources["ButtonStyle"] = this.Resources["DarkButton"];
                Application.Current.Resources["TextBoxStyle"] = this.Resources["DarkTextBox"];
                Application.Current.Resources["ListBoxStyle"] = this.Resources["DarkListBox"];
            }
            else
            {
                NightModeButton.Content = "🌙 Night Mode";
                Application.Current.Resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF6, 0xF5));
                Application.Current.Resources["ContentBackground"] = Brushes.White;
                Application.Current.Resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x3E, 0x50));
                Application.Current.Resources["StatusBackground"] = new SolidColorBrush(Color.FromRgb(0xEC, 0xF0, 0xF1));
                Application.Current.Resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(0xBD, 0xC3, 0xC7));
                Application.Current.Resources["TextForeground"] = Brushes.Black;
                Application.Current.Resources["ButtonStyle"] = this.Resources["LightButton"];
                Application.Current.Resources["TextBoxStyle"] = this.Resources["LightTextBox"];
                Application.Current.Resources["ListBoxStyle"] = this.Resources["LightListBox"];
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(input))
            {
                ProcessInput(input);
                UserInput.Clear();
            }
        }

        private void ProcessInput(string input)
        {
            LogActivity("User Input", input);
            UpdateChatDisplay($"You: {input}\n");

            _memoryService.AddToConversationHistory(input);
            _userMemory.CurrentTopic = DetectTopic(input);

            string response;
            if (IsTaskCommand(input))
            {
                response = ProcessNaturalLanguageTask(input);
            }
            else if (input.ToLower().Contains("show task") || input.ToLower().Contains("my task"))
            {
                response = ShowTasks();
            }
            else if (input.ToLower().Contains("quiz") || input.ToLower().Contains("test me"))
            {
                StartQuiz();
                response = "Starting cybersecurity quiz! Look at the Quiz Game tab.";
            }
            else if (input.ToLower().Contains("log") || input.ToLower().Contains("activity") ||
                     input.ToLower().Contains("what have you done"))
            {
                response = "Recent activities displayed in the Activity Log tab!";
                LoadActivityLog();
            }
            else if (input.ToLower().Contains("password audit"))
            {
                response = PerformPasswordAudit();
            }
            else if (input.ToLower().Contains("phishing test"))
            {
                response = PerformPhishingTest();
            }
            else if (input.ToLower().Contains("threat scan"))
            {
                response = PerformThreatScan();
            }
            else if (input.ToLower().Contains("tip") || input.ToLower().Contains("advice"))
            {
                response = GetRandomTip();
            }
            else if (input.ToLower().Contains("help"))
            {
                response = ShowHelp();
            }
            else if (input.ToLower().StartsWith("add note"))
            {
                response = AddNote(input.Replace("add note", "").Trim());
            }
            else if (input.ToLower().StartsWith("view notes"))
            {
                response = ViewNotes();
            }
            else
            {
                response = ResponseService.GetResponse(input);
            }

            StartTypingResponse($"Bot: {response}\n");
        }

        private bool IsTaskCommand(string input)
        {
            string[] keywords = { "task", "remind", "reminder", "add", "create", "set" };
            return keywords.Any(k => input.Contains(k));
        }

        private string ProcessNaturalLanguageTask(string input)
        {
            var (title, description, dueDate, setReminder) = ParseTaskInput(input);

            string reminderTime = "1 day before";
            AddTask(title, description, dueDate, setReminder, reminderTime);

            return $"✅ Task added: {title}\nDescription: {description}\nDue: {dueDate:MM/dd/yyyy}" +
                   (setReminder ? "\nReminder set!" : "");
        }

        private (string, string, DateTime, bool) ParseTaskInput(string input)
        {
            input = input.ToLower();
            string title = "Security Task";
            string description = "No description provided";
            DateTime dueDate = DateTime.Now.AddDays(1);
            bool setReminder = false;

            // Extract title
            if (input.Contains("add task"))
            {
                title = ExtractBetween(input, "add task", " with") ??
                        ExtractAfter(input, "add task") ??
                        "Security Task";
            }
            else if (input.Contains("remind me to"))
            {
                title = ExtractAfter(input, "remind me to") ?? "Security Task";
            }

            // Extract description
            if (input.Contains("description"))
            {
                description = ExtractBetween(input, "description", " due") ??
                             ExtractAfter(input, "description") ??
                             "No description provided";
            }

            // Extract due date
            if (input.Contains("tomorrow"))
            {
                dueDate = DateTime.Now.AddDays(1);
            }
            else if (input.Contains("next week"))
            {
                dueDate = DateTime.Now.AddDays(7);
            }
            else if (input.Contains("in 3 days"))
            {
                dueDate = DateTime.Now.AddDays(3);
            }
            else if (input.Contains("next month"))
            {
                dueDate = DateTime.Now.AddMonths(1);
            }

            // Check for reminder
            setReminder = input.Contains("remind");

            return (title.Trim(), description.Trim(), dueDate, setReminder);
        }

        private string ExtractBetween(string input, string start, string end)
        {
            int startIndex = input.IndexOf(start);
            if (startIndex == -1) return null;

            startIndex += start.Length;
            int endIndex = input.IndexOf(end, startIndex);
            if (endIndex == -1) return null;

            return input.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private string ExtractAfter(string input, string phrase)
        {
            int startIndex = input.IndexOf(phrase);
            if (startIndex == -1) return null;

            return input.Substring(startIndex + phrase.Length).Trim();
        }

        private string DetectTopic(string input)
        {
            input = input.ToLower();
            if (input.Contains("password")) return "password";
            if (input.Contains("phishing")) return "phishing";
            if (input.Contains("malware")) return "malware";
            if (input.Contains("vpn")) return "vpn";
            if (input.Contains("2fa")) return "2fa";
            if (input.Contains("backup")) return "backup";
            return "general";
        }

        private string ShowTasks()
        {
            if (_tasks.Any(t => !t.IsCompleted))
            {
                var taskList = _tasks.Where(t => !t.IsCompleted)
                    .Select(t => $"- {t.Title} (Due: {t.DueDate:MM/dd/yyyy})");
                return $"Your tasks:\n{string.Join("\n", taskList)}";
            }
            return "You have no pending tasks!";
        }

        private string ShowHelp()
        {
            return "I can help with:\n- Security questions (ask anything!)\n- Adding tasks (\"Add task...\")\n- Cybersecurity quiz (\"Start quiz\")\n" +
                   "- Password audits (\"Password audit\")\n- Phishing tests\n- Threat scans\n- Security tips (\"Give me a tip\")\n" +
                   "- Secure notes (\"Add note...\", \"View notes\")\n- Activity logs (\"Show activity\")";
        }

        private string GetRandomTip()
        {
            _tipsService.DisplayRandomTip();
            return "Check the Security Tools tab for a random security tip!";
        }

        private string PerformPasswordAudit()
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox("Enter password to analyze", "Password Audit");
            if (!string.IsNullOrEmpty(password))
            {
                var audit = new PasswordAuditService().Analyze(password);
                LogActivity("Password Audit", $"Score: {audit.score}/100");
                return $"Password Audit:\nScore: {audit.score}/100\nFeedback: {audit.feedback}";
            }
            return "Password audit canceled.";
        }

        private string PerformPhishingTest()
        {
            var simulator = new PhishingSimulator();
            simulator.StartSimulation("User");
            LogActivity("Phishing Test", "Completed");
            return "Phishing Test completed. Check console for simulation.";
        }

        private string PerformThreatScan()
        {
            var report = _threatScanner.GenerateReport();
            ConsoleHelper.PrintReport(report);
            LogActivity("Threat Scan", "Completed");
            return "Threat Scan completed. See console for details.";
        }

        private string AddNote(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Please provide note content.";

            string title = $"Note {DateTime.Now:MMddHHmm}";
            try
            {
                NoteService.AddNote(title, content);
                LogActivity("Note Added", title);
                return $"Secure note added: {title}";
            }
            catch (Exception ex)
            {
                return $"Failed to add note: {ex.Message}";
            }
        }

        private string ViewNotes()
        {
            try
            {
                var notes = NoteService.GetNotes();
                if (notes.Count == 0) return "You have no secure notes.";

                string result = "Your secure notes:\n";
                foreach (var note in notes)
                {
                    result += $"- {note.Title}: {note.Content.Substring(0, Math.Min(30, note.Content.Length))}...\n";
                }
                return result;
            }
            catch (Exception ex)
            {
                return $"Failed to retrieve notes: {ex.Message}";
            }
        }

        private void AddTask(string title, string description, DateTime dueDate, bool setReminder, string reminderTime)
        {
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                IsCompleted = false
            };

            if (setReminder)
            {
                task.ReminderTime = reminderTime switch
                {
                    "1 day before" => dueDate.AddDays(-1),
                    "3 days before" => dueDate.AddDays(-3),
                    "1 week before" => dueDate.AddDays(-7),
                    _ => dueDate.AddDays(-1)
                };
            }

            _tasks.Add(task);
            UpdateTaskList();
            LogActivity("Task Added", $"{title} (Due: {dueDate:MM/dd/yyyy})");
        }

        private void CheckReminders()
        {
            foreach (var task in _tasks.Where(t =>
                t.ReminderTime.HasValue &&
                !t.IsCompleted &&
                DateTime.Now >= t.ReminderTime.Value))
            {
                ShowReminder(task);
                task.ReminderTime = null;
            }
        }

        private void ShowReminder(TaskItem task)
        {
            StartTypingResponse($"🔔 Reminder: {task.Title}\n" +
                               $"Due: {task.DueDate:MM/dd/yyyy}\n" +
                               $"Description: {task.Description}\n");

            LogActivity("Reminder Triggered", task.Title);
        }

        private void UpdateTaskList()
        {
            TaskList.Items.Clear();
            foreach (var task in _tasks.Where(t => !t.IsCompleted))
            {
                TaskList.Items.Add($"{task.Title} (Due: {task.DueDate:MM/dd/yyyy})");
            }
        }

        private void LogActivity(string action, string details)
        {
            _activityLog.Insert(0, new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Details = details
            });

            if (_activityLog.Count > 10)
                _activityLog.RemoveAt(_activityLog.Count - 1);

            UpdateActivityLogDisplay();
        }

        private void UpdateActivityLogDisplay()
        {
            ActivityLogDisplay.Text = string.Join("\n",
                _activityLog.Select(e =>
                    $"{e.Timestamp:HH:mm} - {e.Action}: {e.Details}"));
        }

        private void LoadActivityLog()
        {
            ActivityLogDisplay.Text = "Recent activities:\n" +
                string.Join("\n", _activityLog.Take(5).Select(e =>
                    $"{e.Timestamp:HH:mm} - {e.Action}: {e.Details}"));
        }

        private void UpdateChatDisplay(string message)
        {
            ChatDisplay.AppendText(message);
            ChatDisplay.ScrollToEnd();
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        private void PasswordAuditButton_Click(object sender, RoutedEventArgs e)
        {
            string result = PerformPasswordAudit();
            StartTypingResponse($"Bot: {result}\n");
        }

        private void PhishingTestButton_Click(object sender, RoutedEventArgs e)
        {
            string result = PerformPhishingTest();
            StartTypingResponse($"Bot: {result}\n");
        }

        private void ThreatScanButton_Click(object sender, RoutedEventArgs e)
        {
            string result = PerformThreatScan();
            StartTypingResponse($"Bot: {result}\n");
        }

        private void TipButton_Click(object sender, RoutedEventArgs e)
        {
            _tipsService.DisplayRandomTip();
            StartTypingResponse("Bot: Check the Security Tools tab for a random security tip!\n");
        }

        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            StartQuiz();
            LogActivity("Quiz", "Started");
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitAnswer();
        }

        private void StartQuiz()
        {
            _quizScore = 0;
            _currentQuestionIndex = 0;
            ShowQuestion(_currentQuestionIndex);
            LogActivity("Quiz", "Started");
        }

        private void ShowQuestion(int index)
        {
            var question = _quizQuestions[index];
            GameTitle.Text = $"Question {index + 1}/{_quizQuestions.Count}";
            GameQuestion.Text = question.Text;
            GameOptions.Children.Clear();

            for (int i = 0; i < question.Options.Count; i++)
            {
                var radioButton = new RadioButton
                {
                    Content = $"{Convert.ToChar(65 + i)}. {question.Options[i]}",
                    Margin = new Thickness(0, 0, 0, 5),
                    GroupName = "QuizOptions",
                    Foreground = (Brush)Application.Current.Resources["TextForeground"]
                };
                GameOptions.Children.Add(radioButton);
            }

            GameTitle.Visibility = Visibility.Visible;
            GameQuestion.Visibility = Visibility.Visible;
            GameOptions.Visibility = Visibility.Visible;
            SubmitAnswerButton.Visibility = Visibility.Visible;
            QuizFeedback.Visibility = Visibility.Collapsed;
            QuizButton.Visibility = Visibility.Collapsed;
        }

        private void SubmitAnswer()
        {
            var selected = GameOptions.Children
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true);

            if (selected != null)
            {
                int selectedIndex = GameOptions.Children.IndexOf(selected);
                bool isCorrect = selectedIndex == _quizQuestions[_currentQuestionIndex].CorrectIndex;

                _quizScore += isCorrect ? 1 : 0;
                QuizFeedback.Text = isCorrect
                    ? "✅ Correct! " + GetExplanation(_currentQuestionIndex)
                    : "❌ Incorrect. " + GetExplanation(_currentQuestionIndex);

                QuizFeedback.Visibility = Visibility.Visible;
                LogActivity("Quiz Answer", $"Q{_currentQuestionIndex + 1} {(isCorrect ? "Correct" : "Wrong")}");

                _currentQuestionIndex++;
                if (_currentQuestionIndex < _quizQuestions.Count)
                {
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, e) => {
                        timer.Stop();
                        ShowQuestion(_currentQuestionIndex);
                    };
                    timer.Start();
                }
                else
                {
                    ShowQuizScore();
                }
            }
        }

        private string GetExplanation(int index) => index switch
        {
            0 => "Reporting phishing emails helps prevent scams.",
            1 => "Urgent demands create pressure to bypass security checks.",
            2 => "Long, complex passwords are hardest to crack.",
            3 => "2FA requires both password and secondary verification.",
            4 => "VPNs encrypt traffic between your device and the internet.",
            5 => "Malicious attachments are common malware vectors.",
            6 => "Unsecured networks expose your data to interception.",
            7 => "Managers create/store strong unique passwords.",
            8 => "Attackers manipulate people into revealing information.",
            9 => "3 copies, 2 media types, 1 offsite backup.",
            _ => "Cybersecurity best practices protect your data."
        };

        private void ShowQuizScore()
        {
            string feedback = _quizScore switch
            {
                >= 8 => "Excellent! You're a cybersecurity expert!",
                >= 5 => "Good job! You have solid security knowledge.",
                _ => "Keep learning - security awareness is crucial!"
            };

            UpdateChatDisplay($"\nQuiz Complete! Score: {_quizScore}/10\n{feedback}\n");
            LogActivity("Quiz Completed", $"Score: {_quizScore}/10");
            ResetQuiz();
        }

        private void ResetQuiz()
        {
            GameTitle.Visibility = Visibility.Collapsed;
            GameQuestion.Visibility = Visibility.Collapsed;
            GameOptions.Children.Clear();
            SubmitAnswerButton.Visibility = Visibility.Collapsed;
            QuizFeedback.Visibility = Visibility.Collapsed;
            QuizButton.Visibility = Visibility.Visible;
            _currentQuestionIndex = -1;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TaskTitleInput.Text))
            {
                AddTask(
                    TaskTitleInput.Text,
                    TaskDescriptionInput.Text,
                    TaskDueDate.SelectedDate ?? DateTime.Now.AddDays(1),
                    TaskReminderCheck.IsChecked ?? false,
                    (TaskReminderTime.SelectedItem as ComboBoxItem)?.Content.ToString()
                );

                TaskTitleInput.Clear();
                TaskDescriptionInput.Clear();
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem != null)
            {
                string taskDisplay = TaskList.SelectedItem.ToString();
                string taskTitle = taskDisplay.Split('(')[0].Trim();
                var task = _tasks.FirstOrDefault(t => t.Title == taskTitle);

                if (task != null)
                {
                    task.IsCompleted = true;
                    LogActivity("Task Completed", task.Title);
                    UpdateTaskList();
                    StartTypingResponse($"✅ Completed task: {task.Title}\n");
                }
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string content = Microsoft.VisualBasic.Interaction.InputBox("Enter note content", "Add Secure Note");
            if (!string.IsNullOrEmpty(content))
            {
                string result = AddNote(content);
                StartTypingResponse($"Bot: {result}\n");
            }
        }

        private void ViewNotesButton_Click(object sender, RoutedEventArgs e)
        {
            string result = ViewNotes();
            StartTypingResponse($"Bot: {result}\n");
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CheckReminders();
        }
    }
}