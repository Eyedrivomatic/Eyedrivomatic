using System;
using System.Collections.Generic;

namespace Eyedrivomatic.Device
{
    public interface ISwitchStatus : IReadOnlyList<bool?>
    {
        event EventHandler<uint> SwitchStateChanged;
    }
}