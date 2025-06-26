using System.Media;
using System.IO;

namespace LeoCyberSafe.Utilities
{
    public static class AudioHelper
    {
        public static void PlayWelcomeSound()
        {
            try
            {
                var soundPath = Path.Combine("Resources", "welcome.wav");
                using var player = new SoundPlayer(soundPath);
                player.PlaySync();
            }
            catch
            {
                Console.WriteLine("(Audio skipped)");
            }
        }
    }
}