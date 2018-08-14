using System;

namespace Eyedrivomatic.Device.Communications
{
    public class DeviceIdFilter : IEquatable<DeviceIdFilter>
    {
        public DeviceIdFilter(string vid, params string[] pids)
        {
            Vid = vid;
            Pids = pids;
        }

        public string Vid { get; }
        public string[] Pids { get; }

        public bool Equals(DeviceIdFilter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Vid, other.Vid) && Equals(Pids, other.Pids);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DeviceIdFilter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Vid != null ? Vid.GetHashCode() : 0) * 397) ^ (Pids != null ? Pids.GetHashCode() : 0);
            }
        }
    }
}