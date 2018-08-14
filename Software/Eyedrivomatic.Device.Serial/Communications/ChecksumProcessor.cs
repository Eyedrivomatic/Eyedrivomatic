//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System.Globalization;
using System.Linq;
using System.Text;

namespace Eyedrivomatic.Device.Serial.Communications
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
