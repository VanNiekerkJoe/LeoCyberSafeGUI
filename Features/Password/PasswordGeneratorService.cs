using System.Security.Cryptography;

namespace LeoCyberSafe.Features.Password
{
    public static class PasswordGeneratorService
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Specials = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        public static string GeneratePassword(int length = 16, bool includeSpecial = true)
        {
            var chars = Lowercase + Uppercase + Digits;
            if (includeSpecial) chars += Specials;

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
        }
    }
}