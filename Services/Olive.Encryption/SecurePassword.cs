namespace Olive.Security
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides secure password hashing service based on PBKDF2. 
    /// </summary>
    public class SecurePassword
    {
        // The following constants may be changed without breaking existing hashes.
        const int SALT_BYTE_SIZE = 64, HASH_BYTE_SIZE = 64;

        static int PBKDF2_ITERATIONS
            => Context.Current.Config.GetValue("Authentication:SecurePassword:Pbkdf2Iterations",
                defaultValue: 10000);

        public string Password { get; set; }
        public string Salt { get; set; }

        /// <summary>
        /// Creates a salted PBKDF2 hash of the password.
        /// </summary>
        public static SecurePassword Create(string password)
        {
            // Generate a random salt
            using (var csprng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SALT_BYTE_SIZE];
                csprng.GetBytes(salt);
                // Hash the password and encode the parameters
                var hashBytes = GetBytes(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);

                return new SecurePassword
                {
                    Password = Convert.ToBase64String(hashBytes),
                    Salt = Convert.ToBase64String(salt)
                };
            }
        }

        /// <summary>
        /// Validates a password given a hash of the correct one.
        /// </summary>
        public static bool Verify(string clearTextPassword, string hashedPassword, string salt)
        {
            if (clearTextPassword.IsEmpty()) return false;
            if (hashedPassword.IsEmpty()) return false;
            if (salt.IsEmpty()) return false;

            var pass = new SecurePassword { Password = hashedPassword, Salt = salt };

            var hashedBytes = pass.GetHashBytes();

            var testHash = GetBytes(clearTextPassword, pass.GetSaltBytes(), PBKDF2_ITERATIONS, hashedBytes.Length);
            return SlowEquals(hashedBytes, testHash);
        }

        static bool SlowEquals(byte[] leftBytes, byte[] rightBytes)
        {
            var diff = (uint)leftBytes.Length ^ (uint)rightBytes.Length;
            for (var i = 0; i < leftBytes.Length && i < rightBytes.Length; i++)
                diff |= (uint)(leftBytes[i] ^ rightBytes[i]);
            return diff == 0;
        }

        static byte[] GetBytes(string password, byte[] salt, int iterations, int outputBytes) =>
            new Rfc2898DeriveBytes(password, salt, iterations).GetBytes(outputBytes);

        byte[] GetHashBytes() => Convert.FromBase64String(Password);

        byte[] GetSaltBytes() => Convert.FromBase64String(Salt);
    }
}