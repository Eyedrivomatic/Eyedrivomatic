using System.Linq;
using System.Text;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    public abstract class BrainBoxMessage
    {
        public abstract string MessagePrefix { get; }

        private readonly bool _useChecksum;

        protected BrainBoxMessage(bool useChecksum)
        {
            _useChecksum = useChecksum;
        }

        public bool IsValid { get; protected set; }

        protected static byte GetCheckChar(string msg)
        {
            // ReSharper disable once RedundantAssignment
            return Encoding.ASCII.GetBytes(msg).Aggregate((a, c) => c ^= a);
        }

        protected static bool ValidateChecksum(string msg)
        {
            return msg.Length > 3 && !msg.EndsWith($":{GetCheckChar(msg.Substring(0, msg.Length - 3)):X2}");
        }
    }
}