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
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Resources;
using Prism.Commands;
using Prism.Mvvm;

namespace Eyedrivomatic.Eyegaze.Configuration.ViewModels
{
    [Export]
    public class EyegazeConfigruationViewModel : BindableBase, IHeaderInfoProvider<string>, IDisposable
    {
        private readonly IDwellClickConfigurationService _dwellClickConfigurationService;
        private readonly IDisposable _saveCommandRegistration;

        public string HeaderInfo => Strings.ViewName_GeneralConfiguration;

        [ImportingConstructor]
        public EyegazeConfigruationViewModel(
            IDwellClickConfigurationService dwellClickConfigurationService, 
            [Import] IEnumerable<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> providers,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand)
        {
            _dwellClickConfigurationService = dwellClickConfigurationService;
            _dwellClickConfigurationService.PropertyChanged += DwellClickConfiguration_PropertyChanged;

            AvailableProviders = providers.Select(factory => factory.Metadata.Name);

            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }


        private static readonly Dictionary<string, string[]> ConfigurationPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IDwellClickConfigurationService.HasChanges), new [] {nameof(HasChanges)} },
            { nameof(IDwellClickConfigurationService.Provider), new [] {nameof(SelectedProvider) } },
            { nameof(IDwellClickConfigurationService.EnableDwellClick), new [] {nameof(DwellClickEnabled) } },
            { nameof(IDwellClickConfigurationService.StandardDwellTimeMilliseconds), new [] {nameof(StandardDwellTimeMilliseconds) } },
            { nameof(IDwellClickConfigurationService.StartButtonDwellTimeMilliseconds), new [] {nameof(StartButtonDwellTimeMilliseconds) } },
            { nameof(IDwellClickConfigurationService.StopButtonDwellTimeMilliseconds), new [] {nameof(StopButtonDwellTimeMilliseconds) } },
            { nameof(IDwellClickConfigurationService.DirectionButtonDwellTimeMilliseconds), new [] {nameof(DirectionButtonsDwellTimeMilliseconds) } },
            { nameof(IDwellClickConfigurationService.DwellTimeoutMilliseconds), new [] {nameof(DwellTimeoutMilliseconds) } },
            { nameof(IDwellClickConfigurationService.RepeatDelayMilliseconds), new [] {nameof(DwellRepeatDelayMilliseconds) } },
        };

        private void DwellClickConfiguration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!ConfigurationPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in ConfigurationPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }
        }

        public IEnumerable<string> AvailableProviders { get; }

        public string SelectedProvider
        {
            get => _dwellClickConfigurationService.Provider;
            set => _dwellClickConfigurationService.Provider = value;
        }

        public bool DwellClickEnabled
        {
            get => _dwellClickConfigurationService.EnableDwellClick;
            set => _dwellClickConfigurationService.EnableDwellClick = value;
        }

        public int StandardDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StandardDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StandardDwellTimeMilliseconds = value;
        }

        public int StartButtonDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StartButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StartButtonDwellTimeMilliseconds = value;
        }

        public int StopButtonDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StopButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StopButtonDwellTimeMilliseconds = value;
        }

        public int DirectionButtonsDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.DirectionButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.DirectionButtonDwellTimeMilliseconds = value;
        }

        public int DwellTimeoutMilliseconds
        {
            get => _dwellClickConfigurationService.DwellTimeoutMilliseconds;
            set => _dwellClickConfigurationService.DwellTimeoutMilliseconds = value;
        }

        public int DwellRepeatDelayMilliseconds
        {
            get => _dwellClickConfigurationService.RepeatDelayMilliseconds;
            set => _dwellClickConfigurationService.RepeatDelayMilliseconds = value;
        }

        public bool HasChanges => _dwellClickConfigurationService.HasChanges;

        public ICommand SaveCommand => new DelegateCommand(() => _dwellClickConfigurationService.Save())
            .ObservesCanExecute(() => HasChanges);

        public void Dispose()
        {
            _saveCommandRegistration?.Dispose();
        }
    }
}
