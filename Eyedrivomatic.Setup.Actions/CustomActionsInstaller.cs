using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;

namespace Eyedrivomatic.Setup.Actions
{
    [RunInstaller(true)]
    public partial class CustomActionsInstaller : Installer
    {
        public CustomActionsInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary savedState)
        {
            InitializeLog();

            base.Install(savedState);

            Log.Info("Starting CustomActionsInstaller.Commit");
            Log.Debug($"Parameters: [{Context.Parameters}]");

            var aclUtils = new AclUtils(Context);
            aclUtils.SetFileSystemPermissions();
        }
        
        private void InitializeLog()
        {
            string logPath = "";
            if (!string.IsNullOrEmpty(Context.Parameters["assemblypath"]))
            {
                var assemblyDir = Path.GetDirectoryName(Context.Parameters["assemblypath"]);
                //This should be the application directory.
                logPath = Path.Combine(assemblyDir, "Logs");
            }

            logPath = Path.Combine(logPath, "InstallLog.txt");
            Log.Initialize(logPath, Context);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Log.Debug("CustomActionsInstaller Disposing");
            if (disposing)
            {
                components?.Dispose();
                Log.Close();
            }
            base.Dispose(disposing);
        }
    }
}
