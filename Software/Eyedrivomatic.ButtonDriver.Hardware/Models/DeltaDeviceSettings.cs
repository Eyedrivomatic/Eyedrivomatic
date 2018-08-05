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


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Device.Models
{
    [Export("Delta", typeof(IDeviceSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DeltaDeviceSettings : BindableBase, IDeviceSettings
    {
        private readonly Func<string, string, Task<bool>> _setConfigurationCommand;
        private readonly Func<Task<bool>> _saveConfigurationCommand;
        private readonly ISettingsMessageSource _settingsMessageSource;
        private readonly Dictionary<string, Action<string>> _messageHandlers;
        private Point? _centerPos;
        private decimal? _maxSpeed;
        private readonly bool?[] _switchDefaults = new bool?[4];
        private DeviceOrientation _orientation = DeviceOrientation.Rotate0Deg;
        public decimal DeviceMaxSpeed => 20;

        [ImportingConstructor]
        internal DeltaDeviceSettings(
            [Import(nameof(IBrainBoxCommands.SetConfiguration))] Func<string, string, Task<bool>> setConfigurationCommand,
            [Import(nameof(IBrainBoxCommands.SaveConfiguration))] Func<Task<bool>> saveConfigurationCommand,
            ISettingsMessageSource settingsMessageSource)
        {
            _setConfigurationCommand = setConfigurationCommand;
            _saveConfigurationCommand = saveConfigurationCommand;
            _settingsMessageSource = settingsMessageSource;
            _settingsMessageSource.SettingsMessageReceived += OnSettingMessage;
            _settingsMessageSource.Disconnected += OnDisconnected;

            // ReSharper disable ExplicitCallerInfoArgument
            _messageHandlers = new Dictionary<string, Action<string>>
            {
                { SettingNames.CenterPos, s => SetProperty(ref _centerPos, Point.Parse(s), nameof(CenterPos))},
                { SettingNames.MaxSpeed, s => SetProperty(ref _maxSpeed, decimal.Parse(s), nameof(MaxSpeed))},
                { SettingNames.Orientation, s => SetProperty(ref _orientation, (DeviceOrientation)int.Parse(s), nameof(Orientation))},
                { SettingNames.Switch1Default, s => SetProperty(ref _switchDefaults[0], s == "ON", nameof(Switch1Default))},
                { SettingNames.Switch2Default, s => SetProperty(ref _switchDefaults[1], s == "ON", nameof(Switch2Default))},
                { SettingNames.Switch3Default, s => SetProperty(ref _switchDefaults[2], s == "ON", nameof(Switch3Default))},
                { SettingNames.Switch4Default, s => SetProperty(ref _switchDefaults[3], s == "ON", nameof(Switch4Default))},
            };
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private void OnSettingMessage(object sender, SettingMessageEventArgs args)
        {
            try
            {
                if (!_messageHandlers.ContainsKey(args.SettingName))
                {
                    Log.Error(this, $"Invalid Setting [{args.SettingName}] received");
                    return;
                }
                Log.Info(this, $"Setting [{args.SettingName}] is [{args.SettingValue}]");
                _messageHandlers[args.SettingName](args.SettingValue);

            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to update [{args.SettingName}]. Value provided was [{args.SettingValue}]. [{ex}]");
            }
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            CenterPos = null;
        }

        public Point? CenterPos
        {
            get => _centerPos;
            set => SendConfiguration(SettingNames.CenterPos, value ?? new Point(0,0));
        }

        public decimal? MaxSpeed
        {
            get => _maxSpeed;
            set => SendConfiguration(SettingNames.MaxSpeed, value ?? 22m);
        }

        public DeviceOrientation Orientation
        {
            get => _orientation;
            set => SendConfiguration(SettingNames.Orientation, (int)value);
        }

        public bool? Switch1Default
        {
            get => _switchDefaults[0];
            set => SendConfiguration(SettingNames.Switch1Default, value ?? false);
        }

        public bool? Switch2Default
        {
            get => _switchDefaults[1];
            set => SendConfiguration(SettingNames.Switch2Default, value ?? false);
        }
        public bool? Switch3Default
        {
            get => _switchDefaults[2];
            set => SendConfiguration(SettingNames.Switch3Default, value ?? false);
        }
        public bool? Switch4Default
        {
            get => _switchDefaults[3];
            set => SendConfiguration(SettingNames.Switch4Default, value ?? false);
        }

        public Task<bool> Save()
        {
            return _saveConfigurationCommand?.Invoke();
        }

        private void SendConfiguration(string name, Point value, [CallerMemberName] string propertyName = null)
        {
            SendConfigurationInternal(name, $"{value.X:F1},{value.Y:F1}", propertyName);
        }

        private void SendConfiguration(string name, decimal value, [CallerMemberName] string propertyName = null)
        {
            SendConfigurationInternal(name, value.ToString("F1"), propertyName);
        }

        private void SendConfiguration(string name, int value, [CallerMemberName] string propertyName = null)
        {
            SendConfigurationInternal(name, value.ToString("0"), propertyName);
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
                Log.Error(this, $"Failed to send setting! [{e}]");

                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(propertyName); //Lets the client know that the "set" failed.
            }
        }

        public void Dispose()
        {
            _settingsMessageSource.SettingsMessageReceived -= OnSettingMessage;
            _settingsMessageSource.Disconnected -= OnDisconnected;
        }
    }
}