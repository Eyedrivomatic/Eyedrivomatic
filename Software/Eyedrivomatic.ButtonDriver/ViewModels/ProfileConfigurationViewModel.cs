using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Common.Extensions;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Gu.Localization;
using NullGuard;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class ProfileConfigurationViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly ExportFactory<Profile> _profileFactory;
        private readonly IDisposable _saveCommandRegistration;
        private Profile _currentProfile;
        private readonly InteractionRequest<IConfirmationWithCustomButtons> _confirmationRequest;

        [ImportingConstructor]
        public ProfileConfigurationViewModel(IHardwareService hardwareService,
            IButtonDriverConfigurationService configurationService, ExportFactory<Profile> profileFactory,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand, 
            InteractionRequest<IConfirmationWithCustomButtons> confirmationRequest)
            : base(hardwareService)
        {
            _configurationService = configurationService;
            _profileFactory = profileFactory;
            _confirmationRequest = confirmationRequest;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;
            _configurationService.DrivingProfiles.CollectionChanged += DrivingProfilesOnCollectionChanged;
            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }

        private void ConfigurationService_PropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        public string HeaderInfo => Strings.ViewName_ProfileConfig;

        public ObservableCollection<Profile> DrivingProfiles => _configurationService.DrivingProfiles;

        [AllowNull]
        public Profile CurrentProfile
        {
            get
            {
                if (_currentProfile == null) CurrentProfile = DrivingProfiles.FirstOrDefault();
                return _currentProfile;
            }
            set
            {
                if (_currentProfile != null) _currentProfile.Speeds.CollectionChanged -= SpeedsOnCollectionChanged;

                SetProperty(ref _currentProfile, value);
                if (_currentProfile != null) _currentProfile.Speeds.CollectionChanged += SpeedsOnCollectionChanged;
            }
        }

        public ICommand AddProfileCommand => new DelegateCommand(InsertProfile);
        public ICommand DeleteProfileCommand => new DelegateCommand<Profile>(DeleteProfile,
            profile => profile != null && DrivingProfiles.Count > 1)
            .ObservesProperty(() => DrivingProfiles);

        public ICommand MoveProfileUpCommand => new DelegateCommand<Profile>(profile => MoveProfile(profile, true),
            profile => profile != null && DrivingProfiles.IndexOf(profile) > 0)
            .ObservesProperty(() => DrivingProfiles);

        public ICommand MoveProfileDownCommand => new DelegateCommand<Profile>(profile => MoveProfile(profile, false),
            profile => profile != null && DrivingProfiles.IndexOf(profile) < DrivingProfiles.Count - 1)
            .ObservesProperty(() => DrivingProfiles);


        public ICommand AddProfileSpeedCommand => new DelegateCommand<ProfileSpeed>(profileSpeed => CurrentProfile.AddSpeed(profileSpeed), profileSpeed => CurrentProfile != null)
            .ObservesProperty(() => CurrentProfile);

        public ICommand DeleteProfileSpeedCommand => new DelegateCommand<ProfileSpeed>(DeleteProfileSpeed, 
            speed => speed != null && CurrentProfile.Speeds.Contains(speed))
            .ObservesProperty(() => CurrentProfile);

        public ICommand MoveSpeedUpCommand => new DelegateCommand<ProfileSpeed>(speed => MoveProfileSpeed(speed, true),
            speed => speed != null && CurrentProfile.Speeds.Contains(speed) && CurrentProfile.Speeds.IndexOf(speed) > 0)
            .ObservesProperty(() => CurrentProfile);

        public ICommand MoveSpeedDownCommand => new DelegateCommand<ProfileSpeed>(speed => MoveProfileSpeed(speed, false),
            speed => speed != null && CurrentProfile.Speeds.Contains(speed) && CurrentProfile.Speeds.IndexOf(speed) < CurrentProfile.Speeds.Count - 1)
            .ObservesProperty(() => CurrentProfile);

        public bool HasChanges => _configurationService.HasChanges;

        public ICommand SaveCommand => new DelegateCommand(() => _configurationService.Save())
            .ObservesCanExecute(() => HasChanges);

        private void MoveProfile(Profile profile, bool forward)
        {
            var index = DrivingProfiles.IndexOf(profile);
            if (index < 0) return;

            DrivingProfiles.Remove(profile);
            DrivingProfiles.Insert(index + (forward ? -1 : 1), profile);
        }

        private void MoveProfileSpeed(ProfileSpeed speed, bool forward)
        {
            if (CurrentProfile == null) return;

            var index = CurrentProfile.Speeds.IndexOf(speed);
            if (index < 0) return;

            CurrentProfile.Speeds.Remove(speed);
            CurrentProfile.Speeds.Insert(index + (forward ? -1 : 1), speed);
        }

        private void InsertProfile()
        {
            var index = CurrentProfile == null ? 0 : DrivingProfiles.IndexOf(CurrentProfile);
            var newProfile = _profileFactory.CreateExport().Value;
            newProfile.Name = newProfile.Name.NextPostfix(DrivingProfiles.Select(profile => profile.Name));
            newProfile.AddDefaultSpeeds();

            DrivingProfiles.Insert(index + 1, newProfile);
            CurrentProfile = newProfile;
        }

        private void DeleteProfile(Profile profile)
        {
            _confirmationRequest.Raise(new ConfirmationWithCustomButtons
            {
                Title = Translate.Key(nameof(Strings.ConfirmDeleteProfile_Title)),
                Content = string.Format(
                    Translate.Key(nameof(Strings.ConfirmDeleteProfile_Directive_Format)),
                    ProfileNameConverter.Convert(profile.Name, typeof(ITranslation), null, null))
            }, 
            confirmation =>
            {
                if (!confirmation.Confirmed) return;
                Log.Info(this, $"Deleting driving profile [{profile.Name}].");
                DrivingProfiles.Remove(profile);
            });
        }

        public LocalizedStringConverter ProfileNameConverter { get; } =
            new LocalizedStringConverter { ResourcePattern = "DriveProfile_{0}" };

        public LocalizedStringConverter ProfileSpeedNameConverter { get; } =
            new LocalizedStringConverter { ResourcePattern = "DriveProfileSpeed_{0}" };

        private void DeleteProfileSpeed(ProfileSpeed speed)
        {
            _confirmationRequest.Raise(new ConfirmationWithCustomButtons
                {
                    Title = Translate.Key(nameof(Strings.ConfirmDeleteProfileSpeed_Title)),
                    Content = string.Format(
                        Translate.Key(nameof(Strings.ConfirmDeleteProfileSpeed_Directive_Format)),
                        ProfileSpeedNameConverter.Convert(speed.Name, typeof(ITranslation), null, null),
                        ProfileNameConverter.Convert(CurrentProfile.Name, typeof(ITranslation), null, null))
                },
                confirmation =>
                {
                    if (!confirmation.Confirmed) return;
                    Log.Info(this, $"Deleting driving speed [{speed.Name}] from profile [{CurrentProfile.Name}.");
                    CurrentProfile.Speeds.Remove(speed);
                });
        }

        private void DrivingProfilesOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(DrivingProfiles));
        }

        private void SpeedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(CurrentProfile));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentProfile = null;
                DrivingProfiles.CollectionChanged -= DrivingProfilesOnCollectionChanged;
                _saveCommandRegistration?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
