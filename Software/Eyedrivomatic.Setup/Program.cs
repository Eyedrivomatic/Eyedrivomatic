using System;
using System.IO;
using WixSharp;
using WixSharp.CommonTasks;
using File = WixSharp.File;

namespace Eyedrivomatic.Setup
{
    internal class Program
    {
        private static void Main()
        {
#if DEBUG
            var build = @"Debug";
            var firmwareVersion = @"2.0.4";
            var setupDir = @"..\..\Setup\Debug";
#else
            var build = @"Release";
            var firmwareVersion = @"2.0.4";
            var setupDir = @"..\..\Setup\";
#endif

            var completeFeature = new Feature("!(loc.CompleteFeatureTitle)", "!(loc.CompleteFeatureDescription)");
            var applicationFeature = new Feature("!(loc.ApplicationFeatureTitle)", "!(loc.ApplicationFeatureDescription)");
            var desktopShortcutFeature = new Feature("!(loc.DesktopShortcutsFeatureTitle)", "!(loc.DesktopShortcutsFeatureDescription)", false);
            var driversFeature = new Feature("!(loc.DriversFeatureTitle)", "!(loc.DriversFeatureDescription)");

            var eyedrivomaticTargetDir = $@"..\Eyedrivomatic\bin\{build}\";
            var resourcesTargetDir = $@"..\Eyedrivomatic.Resources\bin\{build}\";
            var deltaFirmwareTargetDir = $@"..\Eyedrivomatic.Firmware.Delta\{build}\";
            var mk1FirmwareTargetDir = $@"..\Eyedrivomatic.Firmware.Mk1\{build}\";

            var iconfile = $@"{resourcesTargetDir}Images\Logo.ico";
            var libdir = @"..\..\lib\";

            applicationFeature.Add(desktopShortcutFeature);
            completeFeature.Add(applicationFeature);
            completeFeature.Add(driversFeature);

            var project = new ManagedProject("Eyedrivomatic")
            {
                GUID = new Guid("60A2BFED-C726-408B-80A1-6340DFFB4DEE"), //never change
                MajorUpgrade = new MajorUpgrade
                {
                    Schedule = UpgradeSchedule.afterInstallInitialize,
                    AllowSameVersionUpgrades = true,
                    DowngradeErrorMessage = "!(loc.DowngradeErrorMessage)"
                },
                UI = WUI.WixUI_Advanced, //all standard UI dialogs    
                DefaultFeature = completeFeature,
                ControlPanelInfo = new ProductInfo
                {
                    //Comments = "Simple test msi",
                    //Readme = "https://eyedrivomatic.org/manual",
                    //HelpLink = "https://eyedrivomatic.org/support",
                    //HelpTelephone = "111-222-333-444",
                    UrlInfoAbout = "https://eyedrivomatic.org/",
                    //UrlUpdateInfo = "https://eyedrivomatic.org/update",
                    ProductIcon = iconfile,
                    Contact = "support@eyedrivomatic.org",
                    Manufacturer = "Eyedrivomatic.org",
                    InstallLocation = "[INSTALLDIR]",
                },
                BannerImage = @"dialog_banner.png",
                BackgroundImage = @"dialog_background.png",
                ValidateBackgroundImage = false, //https://github.com/oleg-shilo/wixsharp/issues/339
                InstallScope = InstallScope.perMachine
            };
            
            project.AddDir(new Dir(new Id("INSTALLDIR"), applicationFeature, @"%ProgramFiles%\Eyedrivomatic",
                new ExeFileShortcut("Uninstall Eyedrivomatic", "[System64Folder]msiexec.exe", "/x [ProductCode]"),
                new File(new Id("Eyedrivomatic_exe"), $@"{eyedrivomaticTargetDir}Eyedrivomatic.exe",
                    new FileShortcut(desktopShortcutFeature, "%DesktopFolder%") { Advertise = true, WorkingDirectory = "[INSTALL_DIR]", IconFile = iconfile, IconIndex = 0 },
                    new FileShortcut("Eyedrivomatic", @"%ProgramMenu%\Eyedrivomatic") { Advertise = true, WorkingDirectory = "[INSTALL_DIR]", IconFile = iconfile, IconIndex = 0 }),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.exe.config"),
                new File($@"{eyedrivomaticTargetDir}Macros.config"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.Configuration.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.Configuration.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.Macros.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.Macros.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.UI.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.ButtonDriver.UI.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Camera.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Camera.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Configuration.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Configuration.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Controls.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Controls.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Common.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Common.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Common.UI.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Common.UI.pdb"),
                new File($@"{eyedrivomaticTargetDir}GrayscaleEffect.fx"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Configuration.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Configuration.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Delta.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Delta.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Mk1.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Mk1.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Serial.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Device.Serial.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Configuration.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Configuration.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Mouse.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Mouse.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Tobii.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Tobii.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Logging.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Logging.pdb"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Resources.dll"),
                new File($@"{eyedrivomaticTargetDir}Eyedrivomatic.Resources.pdb"),

                //Third party components
                new File($@"{eyedrivomaticTargetDir}Accord.dll"),
                new File($@"{eyedrivomaticTargetDir}Accord.dll.config"),
                new File($@"{eyedrivomaticTargetDir}log4net.dll"),
                new File($@"{eyedrivomaticTargetDir}Microsoft.Practices.ServiceLocation.dll"),
                new File($@"{eyedrivomaticTargetDir}Microsoft.Expression.Interactions.dll"),
                new File($@"{eyedrivomaticTargetDir}ArduinoUploader.dll"),
                new File($@"{eyedrivomaticTargetDir}IntelHexFormatReader.dll"),
                new File($@"{eyedrivomaticTargetDir}RJCP.SerialPortStream.dll"),
                new File($@"{eyedrivomaticTargetDir}Gu.Localization.dll"),
                new File($@"{eyedrivomaticTargetDir}Gu.Wpf.Localization.dll"),
                new File($@"{eyedrivomaticTargetDir}Prism.dll"),
                new File($@"{eyedrivomaticTargetDir}Prism.Mef.Wpf.dll"),
                new File($@"{eyedrivomaticTargetDir}Prism.Wpf.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Windows.Interactivity.dll"),
                new File($@"{eyedrivomaticTargetDir}WpfAnimatedGif.dll"),
                new File($@"{eyedrivomaticTargetDir}Xceed.Wpf.Toolkit.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Threading.Overlapped.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Runtime.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Threading.ThreadPool.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Threading.Thread.dll"),
                new File($@"{eyedrivomaticTargetDir}System.IO.FileSystem.Primitives.dll"),
                new File($@"{eyedrivomaticTargetDir}System.IO.dll"),
                new File($@"{eyedrivomaticTargetDir}System.IO.FileSystem.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Runtime.InteropServices.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Diagnostics.TraceSource.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Collections.Specialized.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Diagnostics.FileVersionInfo.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Security.AccessControl.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Security.Principal.Windows.dll"),
                new File($@"{eyedrivomaticTargetDir}Microsoft.Win32.Registry.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Runtime.Extensions.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Reactive.Interfaces.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Reactive.Windows.Threading.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Reactive.Linq.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Reactive.Core.dll"),
                new File($@"{eyedrivomaticTargetDir}System.Reactive.PlatformServices.dll"),
                new File($@"{eyedrivomaticTargetDir}Accord.Video.DirectShow.dll"),
                new File($@"{eyedrivomaticTargetDir}Accord.Video.dll"),
                new File($@"{eyedrivomaticTargetDir}Tobii.Interaction.Net.dll"),
                new File($@"{eyedrivomaticTargetDir}Tobii.Interaction.Model.dll"),
                new File($@"{eyedrivomaticTargetDir}Tobii.EyeX.Client.dll"),
                new File($@"{eyedrivomaticTargetDir}Tobii.Gaze.Core.Net.dll"),
                new File($@"{eyedrivomaticTargetDir}TobiiGazeCore32.dll"),

                //Localization
                new Dir(@"en-US",
                    new File($@"{eyedrivomaticTargetDir}en-US\Eyedrivomatic.Resources.resources.dll")),
                new Dir(@"de",
                    new File($@"{eyedrivomaticTargetDir}de\Disclaimer.rtf"),
                    new File($@"{eyedrivomaticTargetDir}de\Eyedrivomatic.Resources.resources.dll"),
                    new File($@"{eyedrivomaticTargetDir}de\Microsoft.Expression.Interactions.resources.dll")),
                new Dir(@"en",
                    new File($@"{eyedrivomaticTargetDir}en\Disclaimer.rtf"),
                    //    new File($@"{eyedrivomaticTargetDir}en\Eyedrivomatic.Resources.resources.dll"),
                    new File($@"{eyedrivomaticTargetDir}en\Microsoft.Expression.Interactions.resources.dll")),
                new Dir(@"es-MX",
                    new File($@"{eyedrivomaticTargetDir}es-Mx\Eyedrivomatic.Resources.resources.dll")),
                new Dir(@"sv",
                    new File($@"{eyedrivomaticTargetDir}sv\Eyedrivomatic.Resources.resources.dll")),
                new Dir(@"fr",
                    new File($@"{eyedrivomaticTargetDir}fr\Disclaimer.rtf"),
                    new File($@"{eyedrivomaticTargetDir}fr\Eyedrivomatic.Resources.resources.dll"),
                    new File($@"{eyedrivomaticTargetDir}fr\Microsoft.Expression.Interactions.resources.dll")),

                //Arduino drivers
                new Dir(driversFeature, @"Drivers",
                    new Files($@"{libdir}ArduinoDrivers\*.*")),

                //Firmware
                new Dir(@"Firmware",
                    new File($@"{deltaFirmwareTargetDir}Eyedrivomatic.Firmware.Delta.{firmwareVersion}.hex")
                    {
                        AttributesDefinition = $@"Source={deltaFirmwareTargetDir}Eyedrivomatic.Firmware.Delta.hex;Name=Eyedrivomatic.Firmware.Delta.{firmwareVersion}.hex"
                    })//,
                    //new File($@"{mk1FirmwareTargetDir}Eyedrivomatic.Firmware.Mk1.{firmwareVersion}.hex")
                    //{
                    //    AttributesDefinition = $@"Source={mk1FirmwareTargetDir}Eyedrivomatic.Firmware.Mk.hex;Name=Eyedrivomatic.Firmware.Mk1.{firmwareVersion}.hex"
                    //})
                ));

            project.SetVersionFromFileId(@"Eyedrivomatic_exe");
            project.Feature = completeFeature;
            project.AddProperties(
                new Property("ApplicationFolderName", "Eyedrivomatic Folder"),
                new Property("WixAppFolder", "WixPerMachineFolder"));

            Compiler.EmitRelativePaths = false;
            project.PreserveTempFiles = false;
            project.AfterInstall += Msi_AfterInstall;

            project.Language = "en-US";
            project.OutDir = $@"{setupDir}\{project.Version.ToString(3)}\en\";
            project.OutFileName = "Eyedrivomatic.Setup";
            project.LicenceFile = $@"{eyedrivomaticTargetDir}en\Disclaimer.rtf";
            project.LocalizationFile = "wixui.en.wxl";
            project.BuildMsi();

            project.Language = "de-DE";
            project.OutDir = $@"{setupDir}\{project.Version.ToString(3)}\de\";
            project.OutFileName = "Eyedrivomatic.Setup";
            project.LicenceFile = $@"{eyedrivomaticTargetDir}de\Disclaimer.rtf";
            project.LocalizationFile = "wixui.de.wxl";
            project.BuildMsi();

            project.Language = "fr-FR";
            project.OutDir = $@"{setupDir}\{project.Version.ToString(3)}\fr\";
            project.OutFileName = "Eyedrivomatic.Setup";
            project.LicenceFile = $@"{eyedrivomaticTargetDir}fr\Disclaimer.rtf";
            project.LocalizationFile = "wixui.fr.wxl";
            project.BuildMsi();
        }

        private static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (e.IsInstalling)
            {
                var installer = Path.Combine(e.InstallDir,
                    Environment.Is64BitOperatingSystem
                        ? @"Drivers\dpinst-amd64.exe"
                        : @"Drivers\dpinst-x86.exe");

                if (System.IO.File.Exists(installer))
                {
                    var args = e.UILevel > 2 ? "/SW" : "/Q";
                    System.Diagnostics.Process.Start(installer, args)?.WaitForExit();
                }
            }
        }
    }
}