using System;
using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public class PasswordHasher
    {
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 32; // Adjust hash size based on your security requirements
        private static readonly int Iterations = 10000;

        public static string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive the key from the password and salt using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine the salt and hash and convert to Base64 for storage
                byte[] combinedBytes = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, combinedBytes, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, combinedBytes, SaltSize, HashSize);

                return Convert.ToBase64String(combinedBytes);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Decode the combined salt and hash from Base64
            byte[] combinedBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt and hash from the combined bytes
            byte[] salt = new byte[SaltSize];
            byte[] expectedHash = new byte[HashSize];
            Buffer.BlockCopy(combinedBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(combinedBytes, SaltSize, expectedHash, 0, HashSize);

            // Derive the key from the password and stored salt using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] actualHash = pbkdf2.GetBytes(HashSize);

                // Compare the actual hash with the expected hash
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
        }
    }
}