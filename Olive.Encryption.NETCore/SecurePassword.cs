namespace Olive.Security
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Password hashing/verification with PBKDF2 (SHA-256) and backward-compat support.
    /// New hashes are emitted as PHC-style strings:
    ///   $pbkdf2-sha256$<iterations>$<base64(salt)>$<base64(hash)>
    /// </summary>
    public class SecurePassword
    {
        // --- Tunables for NEW hashes ---
        private const int SALT_BYTE_SIZE = 16;     // 128-bit salt (sufficient; larger is fine)
        private const int HASH_BYTE_SIZE = 64;     // 512-bit derived key (OK with SHA-256)
        private static int TARGET_ITERATIONS =>
            Context.Current.Config.GetValue("Authentication:SecurePassword:Pbkdf2Iterations", defaultValue: 600_000);

        // --- Legacy compatibility ---
        // Your legacy DB stores separate fields: Password (base64 hash) + Salt (base64 salt)
        // and used a configurable iteration count that was historically 10,000 (and may have used SHA-1).
        // Configure the candidates you want to try for legacy verification (order matters).
        private static (HashAlgorithmName algo, int iterations)[] LegacyCandidates()
        {
            // You can override via config if desired (e.g., CSV "sha1:10000;sha256:10000;sha256:200000")
            var fromConfig = Context.Current.Config.GetValue("Authentication:SecurePassword:LegacyCandidates", defaultValue: "")
                ?.Trim();

            if (!string.IsNullOrEmpty(fromConfig))
            {
                try
                {
                    return fromConfig.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(item =>
                        {
                            var parts = item.Split(':', StringSplitOptions.RemoveEmptyEntries);
                            var algoName = parts[0].Trim().ToLowerInvariant();
                            var iters = int.Parse(parts[1].Trim());
                            var algo = algoName switch
                            {
                                "sha1" => HashAlgorithmName.SHA1,
                                "sha256" => HashAlgorithmName.SHA256,
                                "sha512" => HashAlgorithmName.SHA512,
                                _ => throw new NotSupportedException($"Unsupported legacy algo '{algoName}'")
                            };
                            return (algo, iters);
                        })
                        .ToArray();
                }
                catch
                {
                    // Fall through to defaults on parsing problems
                }
            }

            // Reasonable defaults to cover common past setups in your codebase.
            return new[]
            {
                (HashAlgorithmName.SHA256, 10_000), // if you switched to SHA-256 but kept 10k
                (HashAlgorithmName.SHA1,   10_000), // earliest code likely used SHA-1 at 10k
            };
        }

        // --- Create / Hash ---

        /// <summary>
        /// Creates a salted PBKDF2 (SHA-256) hash and returns a PHC-style encoded string.
        /// Store this single string going forward (you can ignore the old "Salt" column).
        /// </summary>
        public static string Create(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            // Allocate salt buffer
            var salt = new byte[SALT_BYTE_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
            // Modern constructor: accepts string + HashAlgorithmName
            using var kdf = new Rfc2898DeriveBytes(
                password,                // string
                salt,                    // byte[]
                TARGET_ITERATIONS,
                HashAlgorithmName.SHA256 // force SHA256
            );
#else
    // Old constructor: only supports SHA1 internally
    var pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);
    using var kdf = new Rfc2898DeriveBytes(
        pwdBytes,                // byte[] password
        salt,                    // byte[] salt
        TARGET_ITERATIONS
    );
#endif

            var hash = kdf.GetBytes(HASH_BYTE_SIZE);

            // Format: $pbkdf2-sha256$<iters>$<saltB64>$<hashB64>
            return $"$pbkdf2-sha256${TARGET_ITERATIONS}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        // --- Verify (new + legacy) ---

        /// <summary>
        /// Verifies a password against either:
        ///   1) a PHC-style encoded hash in 'storedPassword' (preferred new format), OR
        ///   2) a legacy pair (storedPassword=base64 hash, storedSalt=base64 salt).
        /// Returns true/false and sets needsUpgrade when rehash is advisable.
        /// </summary>
        public static bool Verify(string clearTextPassword, string storedPassword, string storedSalt, out bool needsUpgrade)
        {
            needsUpgrade = false;

            if (string.IsNullOrEmpty(clearTextPassword)) return false;
            if (string.IsNullOrEmpty(storedPassword)) return false;

            // Path A: New format (PHC string) has no separate salt column and starts with '$'
            if (storedPassword.StartsWith("$", StringComparison.Ordinal))
            {
                return VerifyPhc(clearTextPassword, storedPassword, out needsUpgrade);
            }

            // Path B: Legacy format requires both fields present
            if (string.IsNullOrEmpty(storedSalt)) return false;

            return VerifyLegacy(clearTextPassword, storedPassword, storedSalt, out needsUpgrade);
        }

        // --- Helpers ---

        private static bool VerifyPhc(string password, string phc, out bool needsUpgrade)
        {
            needsUpgrade = false;

            // Expect: $pbkdf2-sha256$<iters>$<saltB64>$<hashB64>
            var parts = phc.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || parts[0] != "pbkdf2-sha256") return false;

            var iters = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);

            using var kdf = new Rfc2898DeriveBytes(password, salt, iters, HashAlgorithmName.SHA256);
            var actual = kdf.GetBytes(expected.Length);

            var ok = CryptographicOperations.FixedTimeEquals(actual, expected);

            // Suggest upgrade if params are below current target
            if (ok && iters < TARGET_ITERATIONS) needsUpgrade = true;

            return ok;
        }

        private static bool VerifyLegacy(string password, string legacyHashBase64, string legacySaltBase64, out bool needsUpgrade)
        {
            needsUpgrade = false;

            byte[] expected, salt;
            try
            {
                expected = Convert.FromBase64String(legacyHashBase64);
                salt = Convert.FromBase64String(legacySaltBase64);
            }
            catch
            {
                return false;
            }

            foreach (var (algo, iters) in LegacyCandidates())
            {
                using var kdf = new Rfc2898DeriveBytes(password, salt, iters, algo);
                var actual = kdf.GetBytes(expected.Length);

                if (CryptographicOperations.FixedTimeEquals(actual, expected))
                {
                    // Verified using legacy parameters -> prompt rehash
                    needsUpgrade = true;
                    return true;
                }
            }

            return false;
        }
    }
}
