using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eyedrivomatic.Device.Services;

namespace Eyedrivomatic.Device.Serial
{
    [Export(typeof(ISwitchStatus))]
    public class SwitchStatus : ISwitchStatus, IDisposable
    {
        private readonly List<bool?> _switches;
        private readonly IStatusMessageSource _statusMessageSource;

        [ImportingConstructor]
        public SwitchStatus(IStatusMessageSource statusMessageSource, int switchCount)
        {
            _switches = new List<bool?>(switchCount);
            _statusMessageSource = statusMessageSource;
            _statusMessageSource.StatusMessageReceived += OnStatusMessageReceived;
            _statusMessageSource.Disconnected += StatusMessageSourceOnDisconnected;
        }

        private void StatusMessageSourceOnDisconnected(object sender, EventArgs eventArgs)
        {
            for (var iSwitch = 0; iSwitch < _switches.Count; iSwitch++)
            {
                _switches[iSwitch] = null;
            }
        }

        private void OnStatusMessageReceived(object sender, StatusMessageEventArgs e)
        {
            for (var iSwitch = 0; iSwitch < Math.Min(e.Switches.Length, _switches.Count); iSwitch++)
            {
                _switches[iSwitch] = e.Switches[iSwitch];
                SwitchStateChanged?.Invoke(this, (uint)iSwitch+1);
            }
        }

        public IEnumerator<bool?> GetEnumerator()
        {
            return _switches.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _switches.Count;

        public bool? this[int index] =>  index < _switches.Count ? _switches[index-1] : null;

        public event EventHandler<uint> SwitchStateChanged;

        public void Dispose()
        {
            _statusMessageSource.StatusMessageReceived -= OnStatusMessageReceived;
            _statusMessageSource.Disconnected -= StatusMessageSourceOnDisconnected;
        }
    }
}
