﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Eyedrivomatic.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Eyedrivomatic.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connect.
        /// </summary>
        public static string CommandText_Connect {
            get {
                return ResourceManager.GetString("CommandText_Connect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disconnect.
        /// </summary>
        public static string CommandText_Disconnect {
            get {
                return ResourceManager.GetString("CommandText_Disconnect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Next.
        /// </summary>
        public static string CommandText_NextDevice {
            get {
                return ResourceManager.GetString("CommandText_NextDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prev.
        /// </summary>
        public static string CommandText_PrevDevice {
            get {
                return ResourceManager.GetString("CommandText_PrevDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Save.
        /// </summary>
        public static string CommandText_Save {
            get {
                return ResourceManager.GetString("CommandText_Save", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Toggle Relay {0} on-off..
        /// </summary>
        public static string CycleRelayMacroTask_DefaultNameFormat {
            get {
                return ResourceManager.GetString("CycleRelayMacroTask_DefaultNameFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cycle a relay. This changes the state of the relay off-&gt;on-&gt;off with a 200ms delay..
        /// </summary>
        public static string CycleRelayMacroTask_Description {
            get {
                return ResourceManager.GetString("CycleRelayMacroTask_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be a value between 1 and {0}..
        /// </summary>
        public static string CycleRelayMacroTask_InvalidRelay {
            get {
                return ResourceManager.GetString("CycleRelayMacroTask_InvalidRelay", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be greater than 0..
        /// </summary>
        public static string CycleRelayMacroTask_InvalidRepeat {
            get {
                return ResourceManager.GetString("CycleRelayMacroTask_InvalidRepeat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cycle Relay {0}, repeat {1} times with a delay of {2} ms between repeats..
        /// </summary>
        public static string CycleRelayMacroTask_ToStringFormat {
            get {
                return ResourceManager.GetString("CycleRelayMacroTask_ToStringFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delay {0:0.00}s.
        /// </summary>
        public static string DelayTask_DefaultNameFormat {
            get {
                return ResourceManager.GetString("DelayTask_DefaultNameFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delay must be greater than 0..
        /// </summary>
        public static string DelayTask_InvalidDelay {
            get {
                return ResourceManager.GetString("DelayTask_InvalidDelay", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pause for {0:0.00} seconds..
        /// </summary>
        public static string DelayTask_ToStringFormat {
            get {
                return ResourceManager.GetString("DelayTask_ToStringFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I Understand and Agree.
        /// </summary>
        public static string Disclaimer_Button {
            get {
                return ResourceManager.GetString("Disclaimer_Button", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Eyedrivomatic System is potentially dangerous if used without proper caution. No member of the Eyedrivomatic team accept any responsibility whatsoever for any injuries to persons, or damage to property sustained during the use, or misuse of Eyedrivomatic. Use of The Eyedrivomatic System is entirely at the users own risk.
        /// 
        ///To ensure safe operation, the following guidelines MUST be adhered to at all times;
        /// 
        ///Users MUST be supervised at all times. The area of use MUST be entirely flat The area of use  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Disclaimer_Text {
            get {
                return ResourceManager.GetString("Disclaimer_Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DISCLAIMER.
        /// </summary>
        public static string Disclaimer_Title {
            get {
                return ResourceManager.GetString("Disclaimer_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Display Name. The cannot be empty or only whitespace characters..
        /// </summary>
        public static string Macro_InvalidDisplayName {
            get {
                return ResourceManager.GetString("Macro_InvalidDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Display Name. The cannot be empty or only whitespace characters..
        /// </summary>
        public static string MacroTask_InvalidDisplayName {
            get {
                return ResourceManager.GetString("MacroTask_InvalidDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to milliseconds.
        /// </summary>
        public static string MillisecondsLong {
            get {
                return ResourceManager.GetString("MillisecondsLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ms.
        /// </summary>
        public static string MillisecondsShort {
            get {
                return ResourceManager.GetString("MillisecondsShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New Task.
        /// </summary>
        public static string NewTaskViewModelName {
            get {
                return ResourceManager.GetString("NewTaskViewModelName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection Settings.
        /// </summary>
        public static string SettingsGroupName_DeviceConnection {
            get {
                return ResourceManager.GetString("SettingsGroupName_DeviceConnection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dwell Click Settings.
        /// </summary>
        public static string SettingsGroupName_DwellClick {
            get {
                return ResourceManager.GetString("SettingsGroupName_DwellClick", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automatically Connect at Startup.
        /// </summary>
        public static string SettingsName_AutoConnect {
            get {
                return ResourceManager.GetString("SettingsName_AutoConnect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enabled.
        /// </summary>
        public static string SettingsName_DwellClickEnabled {
            get {
                return ResourceManager.GetString("SettingsName_DwellClickEnabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dwell Repeat Delay:.
        /// </summary>
        public static string SettingsName_DwellClickRepeatDelay {
            get {
                return ResourceManager.GetString("SettingsName_DwellClickRepeatDelay", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dwell Time:.
        /// </summary>
        public static string SettingsName_DwellClickTime {
            get {
                return ResourceManager.GetString("SettingsName_DwellClickTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dwell Timeout:.
        /// </summary>
        public static string SettingsName_DwellClickTimeout {
            get {
                return ResourceManager.GetString("SettingsName_DwellClickTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BrainBox.
        /// </summary>
        public static string ViewName_DeviceConfig {
            get {
                return ResourceManager.GetString("ViewName_DeviceConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Indoor Driving.
        /// </summary>
        public static string ViewName_IndoorDriving {
            get {
                return ResourceManager.GetString("ViewName_IndoorDriving", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Actions.
        /// </summary>
        public static string ViewName_Macros {
            get {
                return ResourceManager.GetString("ViewName_Macros", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Edit Actions.
        /// </summary>
        public static string ViewName_MacrosSettings {
            get {
                return ResourceManager.GetString("ViewName_MacrosSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Driving.
        /// </summary>
        public static string ViewName_OutdoorDriving {
            get {
                return ResourceManager.GetString("ViewName_OutdoorDriving", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status.
        /// </summary>
        public static string ViewName_Status {
            get {
                return ResourceManager.GetString("ViewName_Status", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trim.
        /// </summary>
        public static string ViewName_Trim {
            get {
                return ResourceManager.GetString("ViewName_Trim", resourceCulture);
            }
        }
    }
}
