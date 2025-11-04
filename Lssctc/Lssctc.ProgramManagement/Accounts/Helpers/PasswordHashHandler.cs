using System.Security.Cryptography;

namespace Lssctc.ProgramManagement.Accounts.Helpers
{
    public class PasswordHashHandler
    {
        private static int _iterations = 10000;
        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static string HashPassword(string password)
        {
            int saltSize = 128 / 8;
            var salt = new byte[saltSize];
            _rng.GetBytes(salt);
            var subkey = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256).GetBytes(256 / 8);
            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker

            // --- THIS IS THE FIX ---
            // Was: (uint)HashAlgorithmName.SHA256.GetHashCode()
            WriteNetworkByteOrder(outputBytes, 1, (uint)0x01); // Use a stable constant for the PRF

            WriteNetworkByteOrder(outputBytes, 5, (uint)_iterations);
            WriteNetworkByteOrder(outputBytes, 9, (uint)salt.Length);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + salt.Length, subkey.Length);
            return Convert.ToBase64String(outputBytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            byte[] hashBytes;
            try
            {
                hashBytes = Convert.FromBase64String(hash);
            }
            catch
            {
                return false;
            }

            if (hashBytes.Length < 13 || hashBytes[0] != 0x01)
                return false;

            uint algoCode = ReadNetworkByteOrder(hashBytes, 1);

            // --- THIS IS THE FIX ---
            // Was: (uint)HashAlgorithmName.SHA256.GetHashCode()
            if (algoCode != (uint)0x01) // Check against the same stable constant
                return false;

            uint iterations = ReadNetworkByteOrder(hashBytes, 5);
            uint saltLen = ReadNetworkByteOrder(hashBytes, 9);

            if (hashBytes.Length != 13 + saltLen + 32)
                return false;

            byte[] salt = new byte[saltLen];
            Buffer.BlockCopy(hashBytes, 13, salt, 0, (int)saltLen);

            byte[] expectedSubkey = new byte[32];
            Buffer.BlockCopy(hashBytes, 13 + (int)saltLen, expectedSubkey, 0, 32);

            var actualSubkey = new Rfc2898DeriveBytes(password, salt, (int)iterations, HashAlgorithmName.SHA256).GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (actualSubkey[i] != expectedSubkey[i])
                    return false;
            }
            return true;
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset + 0] << 24 | buffer[offset + 1] << 16 | buffer[offset + 2] << 8 | buffer[offset + 3]);
        }
    }
}