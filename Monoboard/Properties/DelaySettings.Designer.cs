﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Monoboard.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    internal sealed partial class DelaySettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static DelaySettings defaultInstance = ((DelaySettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new DelaySettings())));
        
        public static DelaySettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2021-01-01")]
        public global::System.DateTime InfoDelay {
            get {
                return ((global::System.DateTime)(this["InfoDelay"]));
            }
            set {
                this["InfoDelay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2021-01-01")]
        public global::System.DateTime OtherDelay {
            get {
                return ((global::System.DateTime)(this["OtherDelay"]));
            }
            set {
                this["OtherDelay"] = value;
            }
        }
    }
}