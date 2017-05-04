using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Commands;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class ProfileConfigurationViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly ExportFactory<Profile> _profileFactory;
        private Profile _currentProfile;

        [ImportingConstructor]
        public ProfileConfigurationViewModel(IHardwareService hardwareService, IButtonDriverConfigurationService configurationService, ExportFactory<Profile> profileFactory)
            : base(hardwareService)
        {
            _configurationService = configurationService;
            _profileFactory = profileFactory;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;
            _configurationService.DrivingProfiles.CollectionChanged += DrivingProfilesOnCollectionChanged;
        }

        private void ConfigurationService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        public string HeaderInfo => Strings.ViewName_ProfileConfig;

        public ObservableCollection<Profile> DrivingProfiles => _configurationService.DrivingProfiles;

        [AllowNull]
        public Profile CurrentProfile
        {
            get => _currentProfile = _currentProfile ?? DrivingProfiles.FirstOrDefault();
            set => SetProperty(ref _currentProfile, value);
        }

        public ICommand MoveUp => new DelegateCommand<Profile>(profile => MoveProfile(profile, true), profile => profile != null && DrivingProfiles.IndexOf(profile) > 0);
        public ICommand MoveDown => new DelegateCommand<Profile>(profile => MoveProfile(profile, false), profile => profile != null && DrivingProfiles.IndexOf(profile) < DrivingProfiles.Count-1);

        private void MoveProfile(Profile profile, bool forward)
        {
            var index = DrivingProfiles.IndexOf(profile);
            if (index < 0) return;

            DrivingProfiles.Remove(profile);
            DrivingProfiles.Insert(index + (forward ? -1 : 1), profile);
        }

        public ICommand DeleteProfile => new DelegateCommand<Profile>(profile => DrivingProfiles.Remove(profile), profile => profile != null && DrivingProfiles.Count > 1);
        public ICommand AddProfile => new DelegateCommand(InsertProfile);

        private void InsertProfile()
        {
            var index = CurrentProfile == null ? 0 : DrivingProfiles.IndexOf(CurrentProfile);
            var newProfile = _profileFactory.CreateExport();
            DrivingProfiles.Insert(index+1, newProfile.Value);
        }

        private void DrivingProfilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RaisePropertyChanged(string.Empty);
        }
    }
}
