using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Prism.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Hardware.Models
{
    [Export(typeof(IDeviceSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class BrainBoxDeviceSettings : BindableBase, IDeviceSettings
    {
        private readonly Func<string, string, Task<bool>> _setConfigurationCommand;
        private readonly ISettingsMessageSource _settingsMessageSource;
        private readonly Dictionary<string, Action<string>> _messageHandlers;
        private bool _isKnown;
        private int _centerPosX;
        private int _minPosX = -90;
        private int _maxPosX = 90;
        private int _centerPosY;
        private int _minPosY = -90;
        private int _maxPosY = 90;
        private readonly bool[] _switchDefaults = new bool[3];

        [ImportingConstructor]
        internal BrainBoxDeviceSettings(
            [Import(nameof(IBrainBoxCommands.SetConfiguration))] Func<string, string, Task<bool>> setConfigurationCommand,
            ISettingsMessageSource settingsMessageSource)
        {
            Contract.Requires<ArgumentNullException>(setConfigurationCommand != null, nameof(setConfigurationCommand));
            Contract.Requires<ArgumentNullException>(settingsMessageSource != null, nameof(settingsMessageSource));

            _setConfigurationCommand = setConfigurationCommand;
            _settingsMessageSource = settingsMessageSource;
            _settingsMessageSource.SettingsMessageReceived += OnSettingMessage;
            _settingsMessageSource.Disconnected += OnDisconnected;

            // ReSharper disable ExplicitCallerInfoArgument
            _messageHandlers = new Dictionary<string, Action<string>>
            {
                { SettingNames.CenterPosX, s => SetProperty(ref _centerPosX, int.Parse(s), nameof(CenterPosX))},
                { SettingNames.MinPosX, s => SetProperty(ref _minPosX, int.Parse(s), nameof(MinPosX))},
                { SettingNames.MaxPosX, s => SetProperty(ref _maxPosX, int.Parse(s), nameof(MaxPosX))},
                { SettingNames.CenterPosY, s => SetProperty(ref _centerPosY, int.Parse(s), nameof(CenterPosY))},
                { SettingNames.MinPosY, s => SetProperty(ref _minPosY, int.Parse(s), nameof(MinPosY))},
                { SettingNames.MaxPosY, s => SetProperty(ref _maxPosY, int.Parse(s), nameof(MaxPosY))},
                { SettingNames.Switch1Default, s => SetProperty(ref _switchDefaults[0], s == "ON", nameof(Switch1Default))},
                { SettingNames.Switch2Default, s => SetProperty(ref _switchDefaults[1], s == "ON", nameof(Switch2Default))},
                { SettingNames.Switch3Default, s => SetProperty(ref _switchDefaults[2], s == "ON", nameof(Switch3Default))},
            };
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private void OnSettingMessage(object sender, SettingMessageEventArgs args)
        {
            if (!_messageHandlers.ContainsKey(args.SettingName))
            {
                ButtonDriverHardwareModule.Logger?.Log($"Invalid Setting [{args.SettingName} received", Category.Exception, Priority.None);
                IsKnown = false;
                return;
            }
            
            _messageHandlers[args.SettingName](args.SettingValue);
            IsKnown = true;
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            IsKnown = false;
        }

        public bool IsKnown
        {
            get { return _isKnown; }
            private set { SetProperty(ref _isKnown, value); }
        }

        public int CenterPosX
        {
            get { return _centerPosX; }
            set { SendConfiguration(SettingNames.CenterPosX, value); }
        }

        public int MinPosX
        {
            get { return _minPosX; }
            set { SendConfiguration(SettingNames.MinPosX, value); }
        }

        public int MaxPosX
        {
            get { return _maxPosX; }
            set { SendConfiguration(SettingNames.MaxPosX, value); }
        }

        public int CenterPosY
        {
            get { return _centerPosY; }
            set { SendConfiguration(SettingNames.CenterPosY, value); }
        }

        public int MinPosY
        {
            get { return _minPosY; }
            set { SendConfiguration(SettingNames.MinPosY, value); }
        }

        public int MaxPosY
        {
            get { return _maxPosY; }
            set { SendConfiguration(SettingNames.MaxPosY, value); }
        }

        public bool Switch1Default
        {
            get { return _switchDefaults[0]; }
            set { SendConfiguration(SettingNames.Switch1Default, value); }
        }

        public bool Switch2Default
        {
            get { return _switchDefaults[1]; }
            set { SendConfiguration(SettingNames.Switch1Default, value); }
        }
        public bool Switch3Default
        {
            get { return _switchDefaults[2]; }
            set { SendConfiguration(SettingNames.Switch1Default, value); }
        }

        private void SendConfiguration(string name, int value, [CallerMemberName] string propertyName = null)
        {
            SendConfigurationInternal(name, value.ToString("D"), propertyName);
        }

        private void SendConfiguration(string name, bool value, [CallerMemberName] string propertyName = null)
        {
            SendConfigurationInternal(name, value ? "ON" : "OFF", propertyName);
        }

        private async void SendConfigurationInternal(string name, string value, string propertyName)
        {
            try
            {
                await _setConfigurationCommand(name, value);
            }
            catch (Exception e)
            {
                ButtonDriverHardwareModule.Logger?.Log($"Failed to send setting! [{e}]", Category.Exception, Priority.None);

                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(propertyName); //Lets the client know that the "set" failed.
            }
        }

        public void Dispose()
        {
            _settingsMessageSource.SettingsMessageReceived -= OnSettingMessage;
            _settingsMessageSource.Disconnected -= OnDisconnected;
        }
    }
}