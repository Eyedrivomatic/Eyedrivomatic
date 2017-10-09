using System.Globalization;
using System.Linq;
using System.Text;

namespace Eyedrivomatic.Hardware.Communications
{
    public static class ChecksumProcessor
    {
        public static char CheckChar = '#';

        public static bool ValidateChecksum(ref string message)
        {
            if (string.IsNullOrEmpty(message)) return false;

            if (message.Length <= 3 || message[message.Length - 3] != CheckChar) return false; // no checksum

            if (!byte.TryParse(message.Substring(message.Length - 2), NumberStyles.HexNumber, null, out byte checksum))
                return false;

            message = message.Substring(0, message.Length - 3);
            return GetCheckChar(message) == checksum;
        }

        public static string ApplyChecksum(string message)
        {
            return $"{message}{CheckChar}{GetCheckChar(message):X2}";
        }

        public static byte GetCheckChar(string message)
        {
            return Encoding.ASCII.GetBytes(message).Aggregate((a, c) => (byte)(c ^ a));
        }

    }
}
