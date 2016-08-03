// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware
{

    public class UsbSerialDevice
    {
        public string Port { get; set; }

        public string FriendlyName { get; set; }
        public string Description { get; set; }

        public string Vid { get; set; }
        public string Pid { get; set; }
        public string Rev { get; set; }
    }

    public static class UsbSerialDeviceEnumerator
    {
        public static IEnumerable<UsbSerialDevice> EnumerateDevices()
        {
            var hDevInfoSet = NativeMethods.SetupDiGetClassDevs(
                ref NativeMethods.GUID_DEVINTERFACE_SERENUM_BUS_ENUMERATOR,
                null,
                IntPtr.Zero,
                NativeMethods.DiGetClassFlags.DIGCF_PRESENT );

            if (hDevInfoSet.ToInt32() == NativeMethods.INVALID_HANDLE_VALUE)
            {
                yield break;
            }

            try
            {
                var devInfoData = new NativeMethods.DevInfoData {CbSize = (uint)Marshal.SizeOf<NativeMethods.DevInfoData>()};

                for (uint i = 0; NativeMethods.SetupDiEnumDeviceInfo(hDevInfoSet, i, ref devInfoData); i++)
                {
                    var id = GetHardwareIds(hDevInfoSet, devInfoData);

                    var device = new UsbSerialDevice
                    {
                        Port = GetPortName(hDevInfoSet, devInfoData),
                        FriendlyName = GetFriendlyName(hDevInfoSet, devInfoData),
                        Description = GetDescription(hDevInfoSet, devInfoData),
                        Vid = id.ContainsKey("VID") ? id["VID"] : null,
                        Pid = id.ContainsKey("PID") ? id["PID"] : null,
                        Rev = id.ContainsKey("REV") ? id["REV"] : null,
                    };

                    yield return device;
                }

                if (Marshal.GetLastWin32Error() != NativeMethods.NO_ERROR &&
                    Marshal.GetLastWin32Error() != NativeMethods.ERROR_NO_MORE_ITEMS)
                {
                    ButtonDriverHardwareModule.Logger?.Log($"Failed to enumerate USB serial devices. Error: [{Marshal.GetLastWin32Error()}] HR: [{Marshal.GetHRForLastWin32Error()}]", Category.Exception, Priority.None);
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfoSet);
            }
        }

        private static string GetPortName(IntPtr hDevInfoSet, NativeMethods.DevInfoData devInfoData)
        {
            var hRegKey = NativeMethods.SetupDiOpenDevRegKey(
                hDevInfoSet,
                ref devInfoData,
                NativeMethods.DeviceInfoPropertyScope.DICS_FLAG_GLOBAL,
                0,
                NativeMethods.DeviceInfoRegistryKeyType.DIREG_DEV,
                NativeMethods.RegistrySpecificAccessRights.KEY_QUERY_VALUE);

            if (hRegKey == IntPtr.Zero) return string.Empty;

            var safeHandle = new SafeRegistryHandle(hRegKey, true);

            var key = RegistryKey.FromHandle(safeHandle);
            return key.GetValue(@"PortName") as string;
        }

        private static string GetFriendlyName(IntPtr hDevInfoSet, NativeMethods.DevInfoData devInfoData)
        {
            uint propertyType;
            var buffer = new StringBuilder(256);
            uint length = (uint)buffer.Capacity;
            NativeMethods.SetupDiGetDeviceRegistryProperty(hDevInfoSet, ref devInfoData, NativeMethods.DeviceInfoRegistryProperty.SPDRP_FRIENDLYNAME, out propertyType, buffer, length, out length);

            return buffer.ToString();
        }

        private static string GetDescription(IntPtr hDevInfoSet, NativeMethods.DevInfoData devInfoData)
        {
            uint propertyType;
            var buffer = new StringBuilder(256);
            uint length = (uint)buffer.Capacity;
            NativeMethods.SetupDiGetDeviceRegistryProperty(hDevInfoSet, ref devInfoData, NativeMethods.DeviceInfoRegistryProperty.SPDRP_DEVICEDESC, out propertyType, buffer, length, out length);

            return buffer.ToString();
        }


        private static Dictionary<string, string> GetHardwareIds(IntPtr hDevInfoSet, NativeMethods.DevInfoData devInfoData)
        {
            uint propertyType;
            var buffer = new StringBuilder(256);
            uint length = (uint)buffer.Capacity;
            NativeMethods.SetupDiGetDeviceRegistryProperty(hDevInfoSet, ref devInfoData, NativeMethods.DeviceInfoRegistryProperty.SPDRP_HARDWAREID, out propertyType, buffer, length, out length);


            var result = new Dictionary<string, string>();

            var regex = new Regex(@"(?<Enum>[^\\]*)\\((?<ID>[^&]+)&?)+"); //Matches 'USB\VID_123&PID_456&REV_001' or 'root\GenericDevice'

            var match = regex.Match(buffer.ToString());
            if (!match.Success || !match.Groups["ID"].Success) return result; //empty result. The ID group should always match if the match succeeded. But testing here for completeness.

            foreach (var id in match.Groups["ID"].Captures)
            {
                var splitIndex = id.ToString().IndexOf('_');
                if (splitIndex < 0) result.Add("GENERIC", id.ToString());
                else result.Add(id.ToString().Substring(0, splitIndex), id.ToString().Substring(splitIndex+1));
            } 

            return result;
        }

        private static class NativeMethods
        {
            public static Guid GUID_DEVINTERFACE_COMPORT = new Guid(0x86e0d1e0, 0x8089, 0x11d0, 0x9c, 0xe4, 0x08, 0x00, 0x3e, 0x30, 0x1f, 0x73);
            public static Guid GUID_DEVINTERFACE_SERENUM_BUS_ENUMERATOR = new Guid(0x4D36E978, 0xE325, 0x11CE, 0xBF, 0xC1, 0x08, 0x00, 0x2B, 0xE1, 0x03, 0x18);
            public const int NO_ERROR = 0;
            public const int INVALID_HANDLE_VALUE = -1;
            public const int ERROR_NO_MORE_ITEMS = 259;

            [Flags]
            internal enum DiGetClassFlags : uint
            {
                DIGCF_DEFAULT           = 0x00000001, // only valid with DIGCF_DEVICEINTERFACE
                DIGCF_PRESENT           = 0x00000002,
                DIGCF_ALLCLASSES        = 0x00000004,
                DIGCF_PROFILE           = 0x00000008,
                DIGCF_DEVICEINTERFACE   = 0x00000010,
            }

            internal enum DeviceInfoKeyType : ulong
            {
                DIREG_DEV  = 0x00000001, // Open/Create/Delete device key
                DIREG_DRV  = 0x00000002, // Open/Create/Delete driver key
                DIREG_BOTH = 0x00000004  // Delete both driver and Device key
            }

            [Flags]
            internal enum DeviceInfoPropertyScope : uint
            {
                DICS_FLAG_GLOBAL = 0x00000001, // make change in all hardware profiles
                DICS_FLAG_CONFIGSPECIFIC = 0x00000002, // make change in specified profile only
                DICS_FLAG_CONFIGGENERAL = 0x00000004 // 1 or more hardware profile-specific changes to follow.
            }

            internal enum DeviceInfoRegistryKeyType : uint
            {
                DIREG_DEV = 0x00000001, // Open/Create/Delete device key
                DIREG_DRV = 0x00000002, // Open/Create/Delete driver key
                DIREG_BOTH = 0x00000004, // Delete both driver and Device key
            }

            [Flags]
            internal enum StandardAccessRights : uint
            {
                DELETE                      = 0x00010000,
                READ_CONTROL                = 0x00020000,
                WRITE_DAC                   = 0x00040000,
                WRITE_OWNER                 = 0x00080000,
                SYNCHRONIZE                 = 0x00100000,

                STANDARD_RIGHTS_REQUIRED    = 0x000F0000,

                STANDARD_RIGHTS_READ        = READ_CONTROL,
                STANDARD_RIGHTS_WRITE       = READ_CONTROL,
                STANDARD_RIGHTS_EXECUTE     = READ_CONTROL,

                STANDARD_RIGHTS_ALL         = 0x001F0000,
            }

            [Flags]
            internal enum RegistrySpecificAccessRights : uint
            {
                KEY_QUERY_VALUE         = 0x0001,
                KEY_SET_VALUE           = 0x0002,
                KEY_CREATE_SUB_KEY      = 0x0004,
                KEY_ENUMERATE_SUB_KEYS  = 0x0008,
                KEY_NOTIFY              = 0x0010,
                KEY_CREATE_LINK         = 0x0020,
                KEY_WOW64_32KEY         = 0x0200,
                KEY_WOW64_64KEY         = 0x0100,
                KEY_WOW64_RES           = 0x0300,

                KEY_READ                = (StandardAccessRights.STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & ~StandardAccessRights.SYNCHRONIZE,
                KEY_WRITE               = (StandardAccessRights.STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY) & ~StandardAccessRights.SYNCHRONIZE,
                KEY_EXECUTE             = KEY_READ & ~StandardAccessRights.SYNCHRONIZE,

                KEY_ALL_ACCESS = StandardAccessRights.STANDARD_RIGHTS_ALL | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY | KEY_CREATE_LINK & ~StandardAccessRights.SYNCHRONIZE,
            }

            internal enum DeviceInfoRegistryProperty : uint
            {
                //
                // Device registry property codes
                // (Codes marked as read-only (R) may only be used for
                // SetupDiGetDeviceRegistryProperty)
                //
                // These values should cover the same set of registry properties
                // as defined by the CM_DRP codes in cfgmgr32.h.
                //
                // Note that SPDRP codes are zero based while CM_DRP codes are one based!
                //
                SPDRP_DEVICEDESC                  = 0x00000000,  // DeviceDesc = R/W,
                SPDRP_HARDWAREID                  = 0x00000001,  // HardwareID = R/W,
                SPDRP_COMPATIBLEIDS               = 0x00000002,  // CompatibleIDs = R/W,
                SPDRP_UNUSED0                     = 0x00000003,  // unused
                SPDRP_SERVICE                     = 0x00000004,  // Service = R/W,
                SPDRP_UNUSED1                     = 0x00000005,  // unused
                SPDRP_UNUSED2                     = 0x00000006,  // unused
                SPDRP_CLASS                       = 0x00000007,  // Class = R--tied to ClassGUID,
                SPDRP_CLASSGUID                   = 0x00000008,  // ClassGUID = R/W,
                SPDRP_DRIVER                      = 0x00000009,  // Driver = R/W,
                SPDRP_CONFIGFLAGS                 = 0x0000000A,  // ConfigFlags = R/W,
                SPDRP_MFG                         = 0x0000000B,  // Mfg = R/W,
                SPDRP_FRIENDLYNAME                = 0x0000000C,  // FriendlyName = R/W,
                SPDRP_LOCATION_INFORMATION        = 0x0000000D,  // LocationInformation = R/W,
                SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,  // PhysicalDeviceObjectName = R,
                SPDRP_CAPABILITIES                = 0x0000000F,  // Capabilities = R,
                SPDRP_UI_NUMBER                   = 0x00000010,  // UiNumber = R,
                SPDRP_UPPERFILTERS                = 0x00000011,  // UpperFilters = R/W,
                SPDRP_LOWERFILTERS                = 0x00000012,  // LowerFilters = R/W,
                SPDRP_BUSTYPEGUID                 = 0x00000013,  // BusTypeGUID = R,
                SPDRP_LEGACYBUSTYPE               = 0x00000014,  // LegacyBusType = R,
                SPDRP_BUSNUMBER                   = 0x00000015,  // BusNumber = R,
                SPDRP_ENUMERATOR_NAME             = 0x00000016,  // Enumerator Name = R,
                SPDRP_SECURITY                    = 0x00000017,  // Security = R/W, binary form,
                SPDRP_SECURITY_SDS                = 0x00000018,  // Security = W, SDS form,
                SPDRP_DEVTYPE                     = 0x00000019,  // Device Type = R/W,
                SPDRP_EXCLUSIVE                   = 0x0000001A,  // Device is exclusive-access = R/W,
                SPDRP_CHARACTERISTICS             = 0x0000001B,  // Device Characteristics = R/W,
                SPDRP_ADDRESS                     = 0x0000001C,  // Device Address = R,
                SPDRP_UI_NUMBER_DESC_FORMAT       = 0X0000001D,  // UiNumberDescFormat = R/W,
                SPDRP_DEVICE_POWER_DATA           = 0x0000001E,  // Device Power Data = R,
                SPDRP_REMOVAL_POLICY              = 0x0000001F,  // Removal Policy = R,
                SPDRP_REMOVAL_POLICY_HW_DEFAULT   = 0x00000020,  // Hardware Removal Policy = R,
                SPDRP_REMOVAL_POLICY_OVERRIDE     = 0x00000021,  // Removal Policy Override = RW,
                SPDRP_INSTALL_STATE               = 0x00000022,  // Device Install State = R,
                SPDRP_LOCATION_PATHS              = 0x00000023,  // Device Location Paths = R,
                SPDRP_BASE_CONTAINERID            = 0x00000024,  // Base ContainerID = R,

                SPDRP_MAXIMUM_PROPERTY            = 0x00000025,  // Upper bound on ordinals                
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct DevInfoData
            {
                public uint CbSize;
                public Guid ClassGuid;
                public uint DevInst;
                public UIntPtr Reserved;
            }


            [DllImport("SetupAPI.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, [MarshalAs(UnmanagedType.LPTStr)] string enumerator, IntPtr hwndParent, DiGetClassFlags flags);

            [DllImport("SetupAPI.dll", SetLastError = true)]
            internal static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref DevInfoData deviceInfoData);

            [DllImport("SetupAPI.dll", SetLastError = true)]
            internal static extern IntPtr SetupDiOpenDevRegKey(IntPtr deviceInfoSet, ref DevInfoData deviceInfoData, DeviceInfoPropertyScope scope, uint hwProfile, DeviceInfoRegistryKeyType keyType, RegistrySpecificAccessRights samDesired);
            
            [DllImport("SetupAPI.dll", SetLastError = true)]
            internal static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref DevInfoData deviceInfoData, DeviceInfoRegistryProperty property, out uint propertyRegDataType, StringBuilder propertyBuffer, uint propertyBufferSize, out uint requiredSize);

            [DllImport("SetupAPI.dll", SetLastError = true)]
            internal static extern int SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
        }
    }
}
