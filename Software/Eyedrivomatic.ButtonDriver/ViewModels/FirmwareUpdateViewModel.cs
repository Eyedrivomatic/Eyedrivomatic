using System;
using System.Windows.Input;
using Eyedrivomatic.Resources;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class FirmwareUpdateViewModel : BindableBase
    {
        private readonly bool _updateRequired;
        private readonly Version _currentVersion;
        private readonly Version _updateVersion;
        private readonly string _connectionString;

        /// <summary>
        /// Designer constructor.
        /// </summary>
        public FirmwareUpdateViewModel()
        {
            _updateRequired = true;
            _currentVersion = new Version(1, 0);
            _updateVersion = new Version(2, 0, 1);
            _connectionString = "COM1";
        }

        public FirmwareUpdateViewModel(bool updateRequired, Version currentVersion, Version updateVersion, string connectionString)
        {
            _currentVersion = currentVersion;
            _updateVersion = updateVersion;
            _connectionString = connectionString;
            _updateRequired = updateRequired;
        }

        public string Title => _updateRequired ? Strings.Firmware_UpdateRequired_Title : Strings.Firmware_UpdateOptional_Title;

        public string Directive => string.Format(_updateRequired 
            ? Strings.Firmware_UpdateRequired_Directive_Format
            : Strings.Firmware_UpdateOptional_Directive_Format,
            _connectionString);

        public string Versions => string.Format(Strings.Firmware_Update_Versions_Format, _currentVersion, _updateVersion);

        public ICommand ContinueCommand => new DelegateCommand(() => {});
        public ICommand CancelCommand => new DelegateCommand(() => { });
    }
}
