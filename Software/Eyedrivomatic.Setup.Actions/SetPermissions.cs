using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Eyedrivomatic.Setup.Actions
{
    public class AclUtils
    {
        private InstallContext _context;

        public AclUtils(InstallContext context)
        {
            _context = context;
        }

        [SecurityPermission(SecurityAction.Demand)]
        public void SetFileSystemPermissions()
        {
            var parameters = _context.Parameters["SetPermissions"];
            if (string.IsNullOrEmpty(parameters)) return;

            var ruleset = parameters.Split(';');
            foreach (var rule in ruleset)
            {
                var parts = rule.Split('|');
                if (parts.Length < 2) throw new ArgumentException($"Invalid set permissions parameters [{parameters}]");

                var path = parts[0];
                if (string.IsNullOrWhiteSpace(path) || !(File.Exists(path) || Directory.Exists(path)))
                {
                    Log.Error($"Invalid path - [{path}]");
                    return;
                }

                var rights = parts[1];
                if (string.IsNullOrWhiteSpace(rights))
                {
                    Log.Error($"Invalid rights - [{rights}]");
                    return;
                }

                SetFileSystemPermissionsInternal(path, rights);
            }
        }

        private void SetFileSystemPermissionsInternal(string path, string rights, bool allUsers = true)
        {
            var sid = new SecurityIdentifier(allUsers ? WellKnownSidType.BuiltinUsersSid : WellKnownSidType.AuthenticatedUserSid, null);
            SetFileSystemAccessRules(sid, path, GetRightsList(rights, '+'), AccessControlType.Allow);
            SetFileSystemAccessRules(sid, path, GetRightsList(rights, '-'), AccessControlType.Deny);
        }

        private static IEnumerable<FileSystemRights> GetRightsList(string parameters, char access)
        {
            
            var regex = new Regex($@"\{access}([a-zA-Z]+)");
            foreach (var match in regex.Matches(parameters).OfType<Match>())
            {
                FileSystemRights fsr;
                if (Enum.TryParse(match.Groups[1].Value, true, out fsr))
                    yield return fsr;
            }
        }

        private void SetFileSystemAccessRules(SecurityIdentifier sid, string path, IEnumerable<FileSystemRights> rights, AccessControlType access)
        {
            if (!(rights?.Any() ?? false)) return;

            var security = GetFileSystemSecurity(path);
            foreach (var fsr in rights)
            {
                Log.Debug($"Setting {access} [{fsr}] access to [{path}] for SID [{sid}]");
                var rule = new FileSystemAccessRule(sid, fsr, AccessControlType.Allow);
                security.AddAccessRule(rule);
            }

            SetFileSystemSecurity(path, security);
        }

        private static FileSystemSecurity GetFileSystemSecurity(string path)
        {
            return IsPathDirectory(path) 
                ? (FileSystemSecurity) Directory.GetAccessControl(path) 
                : File.GetAccessControl(path);
        }

        private static void SetFileSystemSecurity(string path, FileSystemSecurity security)
        {
            if (IsPathDirectory(path)) Directory.SetAccessControl(path, (DirectorySecurity)security);
            else File.SetAccessControl(path, (FileSecurity)security);
        }


        private static bool IsPathDirectory(string path)
        {
            var attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
