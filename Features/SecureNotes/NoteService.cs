using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LeoCyberSafe.Features.SecureNotes
{
    public static class NoteService
    {
        private static byte[] _encryptionKey;
        private static string _masterPasswordHash;
        private static readonly string NotesPath = "secure_notes.enc";
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initializes the note service with master password
        /// </summary>
        public static void Initialize(string masterPassword)
        {
            if (string.IsNullOrWhiteSpace(masterPassword))
                throw new ArgumentException("Master password cannot be empty");

            // Store hashed password for verification
            using var sha256 = SHA256.Create();
            _masterPasswordHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(masterPassword)));

            // Derive encryption key
            _encryptionKey = DeriveKey(masterPassword);

            // Initialize empty notes file
            if (!File.Exists(NotesPath))
                SaveNotes(new List<Note>());
        }

        /// <summary>
        /// Verifies if the input password matches the master password
        /// </summary>
        public static bool VerifyPassword(string inputPassword)
        {
            using var sha256 = SHA256.Create();
            var inputHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword)));
            return inputHash == _masterPasswordHash;
        }

        /// <summary>
        /// Adds a new encrypted note
        /// </summary>
        public static void AddNote(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Title and content cannot be empty");

            try
            {
                var notes = GetNotes();
                notes.Add(new Note(title, content));
                SaveNotes(notes);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add note", ex);
            }
        }

        /// <summary>
        /// Retrieves all decrypted notes
        /// </summary>
        public static List<Note> GetNotes()
        {
            try
            {
                if (!File.Exists(NotesPath) || new FileInfo(NotesPath).Length == 0)
                    return new List<Note>();

                var encryptedData = File.ReadAllBytes(NotesPath);
                var json = Decrypt(encryptedData);

                return string.IsNullOrWhiteSpace(json)
                    ? new List<Note>()
                    : JsonSerializer.Deserialize<List<Note>>(json, _jsonOptions) ?? new List<Note>();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load notes", ex);
            }
        }

        /// <summary>
        /// Changes the master password and re-encrypts all notes
        /// </summary>
        public static void ChangePassword(string currentPassword, string newPassword)
        {
            if (!VerifyPassword(currentPassword))
                throw new UnauthorizedAccessException("Current password is incorrect");

            var notes = GetNotes();
            Initialize(newPassword);
            SaveNotes(notes);
        }

        private static void SaveNotes(List<Note> notes)
        {
            try
            {
                var json = JsonSerializer.Serialize(notes, _jsonOptions);
                var encrypted = Encrypt(json);
                File.WriteAllBytes(NotesPath, encrypted);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save notes", ex);
            }
        }

        private static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;

            using var ms = new MemoryStream();
            // Write IV first
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return ms.ToArray();
        }

        private static string Decrypt(byte[] cipherText)
        {
            try
            {
                using var aes = Aes.Create();
                using var ms = new MemoryStream(cipherText);

                // Read IV first
                var iv = new byte[16];
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;
                aes.Key = _encryptionKey;

                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static byte[] DeriveKey(string password)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt: Encoding.UTF8.GetBytes("LeoCyberSafeSalt"),
                iterations: 100000,
                HashAlgorithmName.SHA512);
            return pbkdf2.GetBytes(32); // 256-bit key
        }
    }

    /// <summary>
    /// Secure note record containing title and content
    /// </summary>
    public record Note(string Title, string Content);
}