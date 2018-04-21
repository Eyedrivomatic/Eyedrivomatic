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
using System.Windows.Media;
using Accord.Video.DirectShow;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Gu.Localization;
using NullGuard;
using Prism.Commands;
using Prism.Mvvm;

namespace Eyedrivomatic.Camera.ViewModels
{
    [Export]
    public class CameraConfigurationViewModel : BindableBase, IHeaderInfoProvider<string>, IDisposable
    {
        private readonly ICameraConfigurationService _cameraConfigurationService;
        private readonly IDisposable _saveCommandRegistration;

        public string HeaderInfo => Strings.ViewName_CameraConfiguration;

        [ImportingConstructor]
        public CameraConfigurationViewModel(
            ICameraConfigurationService cameraConfigurationService,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand)

        {
            _cameraConfigurationService = cameraConfigurationService;
            _cameraConfigurationService.PropertyChanged += CameraConfigurationPropertyChanged;
            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }

        private void CameraConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(e.PropertyName);
        }

        public bool CameraEnabled
        {
            get => _cameraConfigurationService.CameraEnabled;
            set => _cameraConfigurationService.CameraEnabled = value;
        }

        [AllowNull]
        public FilterInfo Camera
        {
            get => _cameraConfigurationService.Camera;
            set => _cameraConfigurationService.Camera = value;
        }

        [AllowNull]
        public string CameraValue
        {
            get => _cameraConfigurationService.Camera?.MonikerString;
            set => _cameraConfigurationService.Camera = AvailableCameras.FirstOrDefault(c => c.MonikerString == value);
        }

        public Stretch Stretch
        {
            get => _cameraConfigurationService.Stretch;
            set => _cameraConfigurationService.Stretch = value;
        }

        public class StretchItem
        {
            public StretchItem(Stretch value, ITranslation name, ITranslation description)
            {
                Value = value;
                Name = name;
                Description = description;
            }

            public Stretch Value { get; }
            public ITranslation Name{ get; }
            public ITranslation Description { get; }
        }

        public IList<StretchItem> StretchItems { get; } = new[]
        {
            new StretchItem(Stretch.Fill,
                Translate.TranslationFor(nameof(Strings.SettingValue_CameraStretch_Fill)),
                Translate.TranslationFor(nameof(Strings.SettingValueDescription_CameraStretch_Fill))),
            new StretchItem(Stretch.UniformToFill,
                Translate.TranslationFor(nameof(Strings.SettingValue_CameraStretch_UniformToFill)),
                Translate.TranslationFor(nameof(Strings.SettingValueDescription_CameraStretch_UniformToFill))),
            new StretchItem(Stretch.Uniform,
                Translate.TranslationFor(nameof(Strings.SettingValue_CameraStretch_Uniform)),
                Translate.TranslationFor(nameof(Strings.SettingValueDescription_CameraStretch_Uniform))),
        };

        public double OverlayOpacity
        {
            get => _cameraConfigurationService.OverlayOpacity;
            set => _cameraConfigurationService.OverlayOpacity = value;
        }

        public IList<FilterInfo> AvailableCameras => _cameraConfigurationService.AvailableCameras.ToList();

        public bool HasChanges => _cameraConfigurationService.HasChanges;

        public ICommand SaveCommand => new DelegateCommand(() => _cameraConfigurationService.Save())
            .ObservesCanExecute(() => HasChanges);

        public void Dispose()
        {
            _saveCommandRegistration?.Dispose();
        }
    }
}
