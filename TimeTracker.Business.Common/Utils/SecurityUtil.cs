using System.Security.Cryptography;

namespace TimeTracker.Business.Common.Utils
{
    public static class SecurityUtil
    {
        private static readonly string BASE_58_ALBHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly string FULL_ALBHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        private static readonly int SALT_SIZE = 499;
        private static readonly int PASSWORD_SIZE = 12;
        private static readonly int HASH_SIZE = 1023;
        private static readonly int HASH_ITERATIONS = 300;
        
        private static readonly object TimeBasedRandomizerLock = new {};
        private static long LastTokenTicks;
        private static long TokenSequence;

        public static byte[] GenerateSalt(int? size = null)
        {
            var saltSize = size ?? SALT_SIZE;
            var data = new byte[saltSize];
            RandomNumberGenerator.Fill(data);
            return data;
        }
        
        public static string GenerateSaltAsString(int? size = null)
        {
            return Convert.ToBase64String(GenerateSalt(size));
        }

        public static string GeneratePassword(int size)
        {
            return GetBase58RandomString(size);
        }
        
        public static string GeneratePassword(int? size = null)
        {
            var passwordSize = size ?? PASSWORD_SIZE;
            return GetBase58RandomString(passwordSize);
        }

        public static byte[] GeneratePasswordHash(string password, byte[] salt)
        {
            return GeneratePasswordHash(password, salt, HASH_ITERATIONS);
        }

        public static byte[] GeneratePasswordHash(string password, byte[] salt, int iterations)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA1, HASH_SIZE);
        }

        public static string GetBase58RandomString(int length)
        {
            return GetRandomString(length, BASE_58_ALBHABET);
        }

        public static string GetRandomString(int length)
        {
            return GetRandomString(length, FULL_ALBHABET);
        }

        private static string GetRandomString(int Length, String ValidSymbols)
        {
            string randomString = "";
            while (randomString.Length != Length)
            {
                byte[] oneByte = new byte[1];
                RandomNumberGenerator.Fill(oneByte);
                char character = (char)oneByte[0];
                if (ValidSymbols.Contains(character))
                {
                    randomString += character;
                }
            }
            return randomString;
        }
        
        public static string GetTimeBasedToken(bool isShort = false)
        {
            lock (TimeBasedRandomizerLock)
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks == LastTokenTicks)
                {
                    TokenSequence++;
                }
                else
                {
                    LastTokenTicks = nowTicks;
                    TokenSequence = 0;
                }

                IEnumerable<byte> ticksBytes = BitConverter.GetBytes(nowTicks);
                if (isShort)
                {
                    var guidBytes = Guid.NewGuid().ToByteArray();
                    ticksBytes = ticksBytes.Concat(guidBytes);
                }
                else
                {
                    // Prevent collisions when multiple tokens are generated within the same tick.
                    ticksBytes = ticksBytes.Concat(BitConverter.GetBytes(TokenSequence));
                }

                return Convert.ToBase64String(ticksBytes.ToArray())
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace('#', 's');
            }
        }
    }
}
