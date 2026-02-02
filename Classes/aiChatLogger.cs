using System;
using System.Collections.Generic;

namespace smart_attendance_middleware.Classes
{
    public static class aiChatLogger
    {
        // Event to notify when a message should be added
        public static event Action<string, string> OnAddMessage;

        // Event to notify when messages should be cleared
        public static event Action OnClearMessages;

        private static bool _isInitialized = false;

        // Call this method once to initialize the aiChatLogger
        public static void Initialize()
        {
            _isInitialized = true;
        }

        public static void AddMessage(string icon, string message)
        {
            if (!_isInitialized)
            {
                Console.WriteLine("aiChatLogger not initialized. Call Initialize() first.");
                return;
            }

            try
            {
                // Escape the strings for JavaScript
                string escapedIcon = EscapeJavaScriptString(icon);
                string escapedMessage = EscapeJavaScriptString(message);

                // Raise event to notify subscribers (like your form)
                OnAddMessage?.Invoke(escapedIcon, escapedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding message: {ex.Message}");
            }
        }

        public static void ClearMessages()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("aiChatLogger not initialized. Call Initialize() first.");
                return;
            }

            try
            {
                // Raise event to notify subscribers
                OnClearMessages?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing messages: {ex.Message}");
            }
        }

        private static string EscapeJavaScriptString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("'", "\\'")
                       .Replace("\"", "\\\"")
                       .Replace("\r", "")
                       .Replace("\n", "\\n")
                       .Replace("\\", "\\\\");
        }

        // Helper methods for common message types
        public static void AddSuccessMessage(string message)
        {
            AddMessage("✅", message);
        }

        public static void AddWarningMessage(string message)
        {
            AddMessage("⚠️", message);
        }

        public static void AddErrorMessage(string message)
        {
            AddMessage("❌", message);
        }

        public static void AddInfoMessage(string message)
        {
            AddMessage("ℹ️", message);
        }

        public static void AddFingerprintMessage(string message)
        {
            AddMessage("👆", message);
        }

        public static void AddDeviceMessage(string message)
        {
            AddMessage("💻", message);
        }

        public static void AddUserMessage(string message)
        {
            AddMessage("👤", message);
        }

        public static void AddSyncMessage(string message)
        {
            AddMessage("🔄", message);
        }

        public static void AddConnectionMessage(string message)
        {
            AddMessage("📡", message);
        }

        public static void AddTimeMessage(string message)
        {
            AddMessage("⏰", message);
        }

        public static void AddAttendanceMessage(string message)
        {
            AddMessage("📋", message);
        }

        public static void AddDatabaseMessage(string message)
        {
            AddMessage("💾", message);
        }

        public static void AddNetworkMessage(string message)
        {
            AddMessage("🌐", message);
        }

        public static void AddSecurityMessage(string message)
        {
            AddMessage("🔒", message);
        }

        public static void AddConfigurationMessage(string message)
        {
            AddMessage("⚙️", message);
        }

        // AI Personality Methods
        public static void AddWelcomeMessage(string message)
        {
            var greetings = new[]
            {
                $"Hello! I'm your AI assistant, ready to make attendance management effortless!",
                $"Welcome! Let's have a productive day together!",
                $"Hi there! I'm here to help you manage your attendance system seamlessly!"
            };
            
            var random = new Random();
            var selectedGreeting = greetings[random.Next(greetings.Length)];
            AddUserMessage($"{message} {selectedGreeting}");
        }

        public static void AddCelebrationMessage(string achievement)
        {
            AddMessage("🎉", achievement);
        }

        public static void AddMotivationalMessage()
        {
            var messages = new[]
            {
                "Everything is running smoothly! You're doing great! 💪",
                "Your system is performing excellently today! 🌟",
                "All systems green! Ready for another productive day! 🚀"
            };
            
            var random = new Random();
            AddSuccessMessage(messages[random.Next(messages.Length)]);
        }

        public static void AddHelpfulTip(string tip)
        {
            AddMessage("💡", $"Pro tip: {tip}");
        }

        public static void AddGreeting()
        {
            var hour = DateTime.Now.Hour;
            string greeting;
            
            if (hour < 12) greeting = "Good morning";
            else if (hour < 17) greeting = "Good afternoon";
            else greeting = "Good evening";
            
            AddTimeMessage($"{greeting}! Ready to assist you today!");
        }
    }
}