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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Eyedrivomatic.Common;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Configuration.Services;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Prism.Commands;

namespace Eyedrivomatic.Device.Configuration.ViewModels
{
    [Export("Mk1")]
    public class Mk1DeviceConfigruationViewModel : DeviceViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IDeviceConfigurationService _configurationService;
        private readonly IDisposable _saveCommandRegistration;

        [ImportingConstructor]
        public Mk1DeviceConfigruationViewModel(
            IDeviceService deviceService,
            IDeviceConfigurationService configurationService,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand)
            :base(deviceService)
        {
            _configurationService = configurationService;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;

            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);

            TrimCommand = new DelegateCommand<Direction?>(Trim, CanTrim)
                .ObservesProperty(() => TrimPosition)
                .ObservesProperty(() => MaxSpeed);
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public bool Connected => Device?.Connection?.State == ConnectionState.Connected;

        public DelegateCommand<Direction?> TrimCommand { get; } 

        public Point TrimPosition => Device?.DeviceSettings?.CenterPos ?? new Point(0,0);

        protected void Trim(Direction? direction)
        {
            if (!Connected || direction == null || Device.DeviceSettings?.CenterPos == null) return;

            var centerPos = Device.DeviceSettings.CenterPos.Value;

            var actionDictionary = new Dictionary<Direction, Action>
            {
                { Direction.Forward, () => centerPos.Y++ },
                { Direction.Backward, () => centerPos.Y-- },
                { Direction.Right, () => centerPos.X++ },
                { Direction.Left, () => centerPos.X-- }
            };
            if (!actionDictionary.ContainsKey(direction.Value)) return;
            actionDictionary[direction.Value]();

            Device.DeviceSettings.CenterPos = centerPos;
        }

        protected bool CanTrim(Direction? direction)
        {
            if (!Connected || direction == null) return false;

            return (Device?.DeviceSettings?.MaxSpeed ?? 0) > 0;
        }

        public decimal MaxSpeed
        {
            get => Device?.DeviceSettings?.MaxSpeed ?? 0;
            set => Device.DeviceSettings.MaxSpeed = value;
        }
        public decimal DeviceSpeedLimit => Device.DeviceSettings.DeviceMaxSpeed;

        public DeviceOrientation Orientation
        {
            get => Device?.DeviceSettings?.Orientation ?? DeviceOrientation.Rotate0Deg;
            set => Device.DeviceSettings.Orientation = value;
        }

        private bool _deviceHasChanges;

        public bool HasChanges
        {
            get => _configurationService.HasChanges || _deviceHasChanges;
            private set
            {
                _deviceHasChanges = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SaveCommand => new DelegateCommand(Save).ObservesCanExecute(() => HasChanges);

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDeviceStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDeviceStateChanged(sender, e);

            if (e.PropertyName == nameof(Device.Connection))
            {
                RaisePropertyChanged(nameof(Connected));
            }
        }

        private static readonly Dictionary<string, string[]> SettingPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IDeviceSettings.CenterPos),  new [] {nameof(TrimPosition), nameof(MaxSpeed)} },
            { nameof(IDeviceSettings.MaxSpeed), new []{ nameof(MaxSpeed)} },
            { nameof(IDeviceSettings.Orientation), new []{ nameof(Orientation)} },
        };

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverSettingsChanged(sender, e);

            if (!SettingPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in SettingPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }

            if (e.PropertyName == nameof(Device.Connection))
            {
                TrimCommand.RaiseCanExecuteChanged();
            }

            HasChanges = true;
        }

        private static readonly Dictionary<string, string[]> ConfigurationPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IDeviceConfigurationService.HasChanges), new [] {nameof(HasChanges)} },
        };

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private void ConfigurationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ConfigurationPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in ConfigurationPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }
        }

        private async void Save()
        {
            try
            {
                _configurationService.Save();
                await Device.DeviceSettings.Save();
                HasChanges = false;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to save device settings - [{ex}].");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _saveCommandRegistration?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
